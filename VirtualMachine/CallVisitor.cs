using Compiler.Common.Types;
using Compiler.Utilities;
using System.Diagnostics.Contracts;
using VirtualMachine.Common;

namespace VirtualMachine
{
    public class CallVisitor
    {
        public int ArgCount { get; init; }

        public ExecutionEngine Engine { get; init; }

        public CallVisitor(int argCount, ExecutionEngine engine)
        {
            ArgCount = argCount;
            Engine = engine;
        }

        [Pure]
        public bool Visit(NativeFunction native)
        {
            var startIdx = Engine.Stack.Count - ArgCount;
            var count = Engine.Stack.Count - (Engine.Stack.Count - ArgCount);
            var result = native.Function(ArgCount, Engine.Stack.GetRange(startIdx, count));

            try
            {
                Engine.Stack.Resize(Engine.Stack.Count - ArgCount - 1);
                Engine.Stack.EnsureCapacity(Constants.STACK_MAX);
                Engine.Stack.Add(result);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [Pure]
        public bool Visit(Closure closure)
        {
            return Engine.Call(closure, ArgCount);
        }
    }
}
