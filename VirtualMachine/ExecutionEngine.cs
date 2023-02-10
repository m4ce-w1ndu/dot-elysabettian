using Compiler;
using Compiler.Common;
using Compiler.Common.Types;
using Compiler.Utilities;
using VirtualMachine.Common;

using CUpvalue = Compiler.Common.Types.Upvalue;

namespace VirtualMachine
{
    public class ExecutionEngine
    {
        public List<Value> Stack { get; set; }

        private List<CallFrame> Frames { get; set; }

        private Dictionary<string, Value> globals;

        private CUpvalue? openUpvalues;

        public ExecutionEngine()
        {
            Stack = new();
            Frames = new();
            globals = new();
            openUpvalues = null;
            Stack.Capacity = Constants.STACK_MAX;
        }

        private void ResetStack()
        {
            Stack.Clear();
            Frames.Clear();
            Stack.Capacity = Constants.STACK_MAX;
            openUpvalues = null;
        }

        private void Push(Value value)
        {
            Stack.Add(value);
        }

        private Value Pop()
        {
            var value = Stack.Last();
            Stack.RemoveAt(Stack.Count - 1);
            return value;
        }

        private Value Peek(int distance)
        {
            return Stack[Stack.Count - 1 - distance];
        }

        private bool Invoke(string name, int argCount)
        {
            try
            {
                var instance = Peek(argCount).Get<Instance>();
                instance!.Fields.TryGetValue(name, out Value? value);

                if (value is not null)
                {
                    Stack[Stack.Count - argCount - 1] = value;
                    return CallValue(value, argCount);
                }

                return InvokeFromClass(instance.Class, name, argCount);
            }
            catch (Exception)
            {
                RuntimeError("Only instances of classes have bound methods.");
                return false;
            }
        }

        private bool InvokeFromClass(Class classValue, string name, int argCount)
        {
            classValue.Methods.TryGetValue(name, out Closure? value);
            if (value is null)
            {
                RuntimeError($"Undefined property '{name}.");
                return false;
            }

            var method = value;
            return Call(method, argCount);
        }

        public bool CallValue(Value callee, int argCount)
        {
            var visitor = new CallVisitor(argCount, this);
            // return visitor.Visit()
            return false;
        }

        public bool Call(Closure closure, int argCount)
        {
            if (argCount != closure.Function.Arity)
            {
                RuntimeError($"Expected {closure.Function.Arity} but got {argCount}");
                return false;
            }

            if (Frames.Count + 1 == Constants.FRAMES_MAX)
            {
                RuntimeError("Stack Overflow");
                return false;
            }

            Frames.Add(new CallFrame(closure, 0, Stack.Count - argCount - 1));
            return true;
        }

        private bool BindMethod(Class classValue, string name)
        {
            classValue.Methods.TryGetValue(name, out Closure? value);
            if (value is null)
            {
                RuntimeError($"Undefined property '{name}'.");
                return false;
            }

            var method = value;
            var instance = Peek(0).Get<Instance>();
            var bound = new Method(instance!, method);

            Pop();
            Push(bound);

            return true;
        }

        private CUpvalue CaptureUpvalue(Value local)
        {
            CUpvalue? prevUpvalue = null;
            var upvalue = openUpvalues;

            while (upvalue is not null && upvalue.Location != local)
            {
                prevUpvalue = upvalue;
                upvalue = upvalue.Next;
            }

            if (upvalue is not null && upvalue.Location == local)
                return upvalue;

            var newUpvalue = new CUpvalue(local);
            newUpvalue.Next = upvalue;

            if (prevUpvalue is null)
                openUpvalues = newUpvalue;
            else
                prevUpvalue.Next = newUpvalue;

            return newUpvalue;
        }

        private void CloseUpvalues(Value last)
        {
            while (openUpvalues is not null && openUpvalues.Location != last)
            {
                var upvalue = openUpvalues;
                upvalue.Closed = upvalue.Location;
                upvalue.Location = upvalue.Closed;
                openUpvalues = upvalue.Next;
            }
        }

        private void DefineMethod(string name)
        {
            var method = Peek(0).Get<Closure>();
            var classValue = Peek(1).Get<Class>();
            classValue!.Methods[name] = method!;
            Pop();
        }

        public ExecutionResult Interpret(string source)
        {
            var parser = new Compiler.Compiler(source);
            var opt = parser.Compile();

            if (opt is null) return ExecutionResult.CompileError;

            var function = opt!;
            var closure = new Closure(function);
            Push(closure);
            Call(closure, 0);

            return Run();
        }

        private void DefineNative(string name, Func<int, IEnumerable<Value>, Value> function)
        {
            var obj = new NativeFunction(function);
            if (!globals.ContainsKey(name))
                globals.Add(name, obj);
            else
                globals[name] = obj;
        }

        private void DoublePopAndPush(Value v)
        {
            Pop();
            Pop();
            Push(v);
        }

        private bool BinaryOperation(Func<double, double, Value> op)
        {
            try
            {
                var b = Peek(0).Get<double>();
                var a = Peek(1).Get<double>();

                DoublePopAndPush(op(a, b));
                return true;
            }
            catch (Exception)
            {
                RuntimeError("Operands must be numbers.");
                return false;
            }
        }

        private ExecutionResult Run()
        {
            var readByte = () =>
            {
                return Frames.Last().Closure.Function.GetCode(Frames.Last().InstructionPtr++);
            };

            var readConst = () =>
            {
                return Frames.Last().Closure.Function.GetConst(readByte());
            };

            var readShort = () =>
            {
                Frames.Last().InstructionPtr += 2;
                var first = (Frames.Last().Closure.Function.GetCode(Frames.Last().InstructionPtr - 2) << 8);
                var second = (Frames.Last().Closure.Function.GetCode(Frames.Last().InstructionPtr - 1));
                return (short)(first | second);
            };

            var readString = () =>
            {
                return readConst().Get<string>();
            };

            while (true)
            {
                var instruction = readByte().GetInstruction();
                switch (instruction)
                {
                    case Instruction.Const:
                    {
                        var constant = readConst();
                        Push(constant);
                        break;
                    }

                    case Instruction.Nop: Push(new Monostate()); break;

                    case Instruction.True: Push(true); break;
                    case Instruction.False: Push(false); break;

                    case Instruction.Pop: Pop(); break;

                    case Instruction.GetLoc:
                    {
                        var slot = readByte();
                        Push(Stack[Frames.Last().StackPtr + slot]);
                        break;
                    }

                    case Instruction.GetGlob:
                    {
                        var name = readString();
                        globals.TryGetValue(name, out Value? value);
                        if (value is null)
                        {
                            RuntimeError($"Undefined variable '{name}'.");
                            return ExecutionResult.RuntimeError;
                        }

                        Push(value);
                        break;
                    }

                    case Instruction.DefGlob:
                    {
                        var name = readString();
                        globals[name] = Peek(0);
                        Pop();
                        break;
                    }

                    case Instruction.SetLoc:
                    {
                        var slot = readByte();
                        Stack[Frames.Last().StackPtr + slot] = Peek(0);
                        break;
                    }

                    case Instruction.SetGlob:
                    {
                        var name = readString();
                        if (!globals.ContainsKey(name))
                        {
                            RuntimeError($"Undefined variable '{name}'.");
                            return ExecutionResult.RuntimeError;
                        }

                        globals[name] = Peek(0);
                        break;
                    }

                    case Instruction.GetUpv:
                    {
                        var slot = readByte();
                        Push(Frames.Last().Closure.Upvalues[slot]!.Location);
                        break;
                    }

                    case Instruction.SetUpv:
                    {
                        var slot = readByte();
                        Frames.Last().Closure.Upvalues[slot]!.Location = Peek(0);
                        break;
                    }

                    case Instruction.GetProp:
                    {
                        Instance? instance;

                        try
                        {
                            instance = Peek(0).Get<Instance>();
                        }
                        catch (Exception)
                        {
                            RuntimeError("Only instances of classes can have retrievable properties.");
                            return ExecutionResult.RuntimeError;
                        }

                        var name = readString();
                        instance!.Fields.TryGetValue(name, out var value);
                        if (value is not null)
                        {
                            Pop();
                            Push(value);
                            break;
                        }

                        if (!BindMethod(instance.Class, name))
                            return ExecutionResult.RuntimeError;

                        break;
                    }

                    case Instruction.SetProp:
                    {
                        try
                        {
                            var instance = Peek(1).Get<Instance>();
                            var name = readString();
                            instance!.Fields[name] = Peek(0);

                            var value = Pop();
                            Pop();
                            Push(value);
                        }
                        catch (Exception)
                        {
                            RuntimeError("Only instances of classes can have fields.");
                            return ExecutionResult.RuntimeError;
                        }
                        break;
                    }

                    case Instruction.GetSup:
                    {
                        var name = readString();
                        var super = Pop().Get<Class>();

                        if (!BindMethod(super!, name))
                            return ExecutionResult.RuntimeError;

                        break;
                    }

                    case Instruction.Eq:
                    {
                        DoublePopAndPush(Peek(0).Equals(Peek(1)));
                        break;
                    }

                    case Instruction.Gt: BinaryOperation((double a, double b) => a > b); break;

                    case Instruction.Lt: BinaryOperation((double a, double b) => a < b); break;

                    case Instruction.Add:
                    {
                        var a = Peek(0);
                        var b = Peek(1);

                        if (a.Get<object>() is double ad && b.Get<object>() is double bd)
                        {
                            DoublePopAndPush(ad + bd);
                            break;
                        }
                        else if (a.Get<object>() is string sa && b.Get<object>() is string sb)
                        {
                            DoublePopAndPush(sa + sb);
                            break;
                        }
                        else if (a.Get<object>() is double add && b.Get<object>() is string sbb)
                        {
                            DoublePopAndPush(add.ToString() + sbb);
                            break;
                        }
                        else if (a.Get<object>() is string saa && b.Get<object>() is double dbb)
                        {
                            DoublePopAndPush(saa + dbb.ToString());
                            break;
                        }
                        else
                        {
                            RuntimeError("Operands must be numbers or strings.");
                            return ExecutionResult.RuntimeError;
                        }
                    }

                    case Instruction.BwNot:
                    {
                        try
                        {
                            var value = (int)Peek(0).Get<double>();
                            Pop();
                            Push(~value);
                        }
                        catch (Exception)
                        {
                            RuntimeError("Operand must be a number.");
                            return ExecutionResult.RuntimeError;
                        }
                        break;
                    }

                    case Instruction.Neg:
                    {
                        try
                        {
                            var negated = -(Peek(0).Get<double>());
                            Pop();
                            Push(negated);
                        }
                        catch (Exception)
                        {
                            RuntimeError("Operand must be a number.");
                            return ExecutionResult.RuntimeError;
                        }
                        break;
                    }

                    case Instruction.Pnt:
                    {
                        Console.WriteLine(Pop());
                        break;
                    }

                    case Instruction.Jmp:
                    {
                        var offset = readShort();
                        Frames.Last().InstructionPtr += offset;
                        break;
                    }

                    case Instruction.Branch:
                    {
                        var offset = readShort();
                        Frames.Last().InstructionPtr -= offset;
                        break;
                    }

                    case Instruction.JmpZ:
                    {
                        var offset = readShort();
                        if (IsFalse(Peek(0)))
                            Frames.Last().InstructionPtr += offset;
                        break;
                    }

                    case Instruction.Call:
                    {
                        var argCount = readByte();
                        if (!CallValue(Peek(argCount), argCount))
                            return ExecutionResult.RuntimeError;
                        break;
                    }

                    case Instruction.Invoke:
                    {
                        var method = readString();
                        var argCount = readByte();
                        if (!Invoke(method, argCount))
                            return ExecutionResult.RuntimeError;
                        break;
                    }

                    case Instruction.SupInvoke:
                    {
                        var method = readString();
                        var argCount = readByte();
                        var super = Pop().Get<Class>();
                        if (!InvokeFromClass(super!, method, argCount))
                            return ExecutionResult.RuntimeError;
                        break;
                    }

                    case Instruction.Closure:
                    {
                        var function = readConst().Get<Function>();
                        var closure = new Closure(function!);
                        Push(closure);
                        for (int i = 0; i < closure.Upvalues.Count; i++)
                        {
                            var isLocal = readByte() == 1 ? true : false;
                            var index = readByte();

                            if (isLocal)
                                closure.Upvalues[i] = CaptureUpvalue(Stack[Frames.Last().StackPtr + index]);
                            else
                                closure.Upvalues[i] = Frames.Last().Closure.Upvalues[index];
                        }

                        break;
                    }

                    case Instruction.CloseUpv:
                    {
                        CloseUpvalues(Stack.Last());
                        Pop();
                        break;
                    }

                    case Instruction.Ret:
                    {
                        var result = Pop();
                        CloseUpvalues(Stack[Frames.Last().StackPtr]);

                        var lastOffset = Frames.Last().StackPtr;
                        Frames.RemoveAt(Frames.Count - 1);

                        if (Frames.Count == 0)
                        {
                            Pop();
                            return ExecutionResult.Ok;
                        }

                        Stack.Resize(lastOffset);
                        Stack.EnsureCapacity(Constants.STACK_MAX);
                        Push(result);
                        break;
                    }

                    case Instruction.ArrBuild:
                    {
                        var newArr = new Compiler.Common.Types.Array();
                        var itemCount = readByte();
                        Push(newArr);

                        for (int i = itemCount; i > 0; i--)
                        {
                            newArr.Values.Add(Peek(i));
                        }

                        Pop();

                        while (itemCount-- > 0)
                            Pop();

                        Push(newArr);
                        break;
                    }

                    case Instruction.ArrIdx:
                    {
                        try
                        {
                            Compiler.Common.Types.Array list;
                            var index = (int)Pop().Get<double>();

                            try
                            {
                                list = Pop().Get<Compiler.Common.Types.Array>()!;
                            }
                            catch (Exception)
                            {
                                RuntimeError("Object is not an array.");
                                return ExecutionResult.RuntimeError;
                            }

                            if (index < 0 || index >= list.Values.Count)
                            {
                                RuntimeError("Array index out of range.");
                                return ExecutionResult.RuntimeError;
                            }

                            var result = list.Values[index];
                            Push(result);
                        }
                        catch (Exception)
                        {
                            RuntimeError("Index is not a number.");
                            return ExecutionResult.RuntimeError;
                        }

                        break;
                    }

                    case Instruction.ArrStore:
                    {
                        try
                        {
                            var item = Pop();
                            var index = (int)Pop().Get<double>();
                            Compiler.Common.Types.Array list;

                            try
                            {
                                list = Pop().Get<Compiler.Common.Types.Array>()!;
                            }
                            catch (Exception)
                            {
                                RuntimeError("Object is not an array.");
                                return ExecutionResult.RuntimeError;
                            }

                            if (index < 0 || index >= list.Values.Count)
                            {
                                RuntimeError("Array index out of bounds.");
                                return ExecutionResult.RuntimeError;
                            }

                            list.Values[index] = item;
                            Push(item);
                        }
                        catch (Exception)
                        {
                            RuntimeError("Index is not a number.");
                            return ExecutionResult.RuntimeError;
                        }
                        break;
                    }

                    case Instruction.Class:
                    {
                        Push(new Class(readString()));
                        break;
                    }

                    case Instruction.Inherit:
                    {
                        try
                        {
                            var superclass = Peek(1).Get<Class>();
                            var subclass = Peek(0).Get<Class>();
                            subclass!.Methods = superclass!.Methods;
                            Pop();
                        }
                        catch (Exception)
                        {
                            RuntimeError("Superclass must be a class.");
                            return ExecutionResult.RuntimeError;
                        }
                        break;
                    }

                    case Instruction.Method:
                    {
                        DefineMethod(readString());
                        break;
                    }
                }
            }
        }

        private void RuntimeError(string message)
        {
            Console.Error.WriteLine(message);
            for (var i = Frames.Count; i-- > 0;)
            {
                var frame = Frames[i];
                var function = frame.Closure.Function;
                var line = function.Chunk.GetLine(frame.InstructionPtr - 1);
                Console.Error.Write($"[line {line} ] in ");
                if (string.IsNullOrEmpty(function.Name))
                    Console.Error.Write("main");
                else
                    Console.Error.WriteLine($"{function.Name}()");
            }

            ResetStack();
        }

        private static bool IsFalse(Value v)
        {
            if (v.Get<object>() is bool b)
                return !b;

            if (v.Get<object>() is Monostate)
                return true;

            return false;
        }
    }
}
