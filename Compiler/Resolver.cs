using Compiler.Common;
using Compiler.Common.Types;

namespace Compiler
{
    /// <summary>
    /// Represents a Local value
    /// </summary>
    public class Local
    {
        /// <summary>
        /// Name of the local value
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Depth (scope) of the local value
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Checks if captured
        /// </summary>
        public bool IsCaptured { get; set; }

        /// <summary>
        /// Constructs a new local value
        /// </summary>
        /// <param name="name">Name of the local value.</param>
        /// <param name="depth">Depth of the local value.</param>
        public Local(string name, int depth)
        {
            Name = name;
            Depth = depth;
            IsCaptured = false;
        }
    }

    /// <summary>
    /// Internal compiler Upvalue implementation
    /// </summary>
    public class Upvalue
    {
        /// <summary>
        /// Index of the upvalue.
        /// </summary>
        public byte Index { get; set; }

        /// <summary>
        /// Locality of the upvalue.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Constructs a new Upvalue
        /// </summary>
        /// <param name="index">Index of the upvalue.</param>
        /// <param name="isLocal">Locality of the upvalue.</param>
        public Upvalue(byte index, bool isLocal)
        {
            Index = index;
            IsLocal = isLocal;
        }
    }

    /// <summary>
    /// Resolves values.
    /// </summary>
    public class Resolver
    {
        public Parser Parser { get; set; }

        public FunctionType Type { get; set; }

        public Function Function { get; set; }

        public Resolver? Enclosing { get; set; }

        public List<Local> Locals { get; set; }

        public List<Upvalue> Upvalues { get; set; }

        public int ScopeDepth { get; set; }

        public Resolver(Parser parser, FunctionType type, Resolver? enclosing)
        {
            Parser = parser;
            Type = type;
            Enclosing = enclosing;
            Locals = new();
            Upvalues = new();
            Function = new Function(0, "");

            Locals.Add(new Local(type == FunctionType.Function ? "" : "this", 0));
            //if (type != FunctionType.Main)
            //    Function.Name = parser.Previous.Text;
        }

        /// <summary>
        /// Adds a new local variable
        /// </summary>
        /// <param name="name">Name of the local variable</param>
        public void AddLocal(string name)
        {
            if (Locals.Count == Value.UINT8_COUNT)
            {
                Parser.Error($"Function exceeds maximum number of local variables." +
                    $"Max number is {Value.UINT8_COUNT - 1}.");
                return;
            }

            Locals.Add(new Local(name, -1));
        }

        /// <summary>
        /// Declares a new local variable
        /// </summary>
        /// <param name="name">Name of the local variable</param>
        public void DeclareVariable(string name)
        {
            if (ScopeDepth == 0) return;

            for (var i = Locals.Count - 1; i >= 0; i--)
            {
                if (Locals[i].Depth != -1 && Locals[i].Depth < ScopeDepth) break;
                if (Locals[i].Name == name)
                    Parser.Error($"Redefinition error. Variable '{name}' already exists" +
                        $" in this scope");
            }

            AddLocal(name);
        }

        /// <summary>
        /// Marks a value as initialized
        /// </summary>
        public void MarkInitialized()
        {
            if (ScopeDepth == 0) return;
            Locals.Last().Depth = ScopeDepth;
        }

        /// <summary>
        /// Resolves a local value
        /// </summary>
        /// <param name="name">Name of the value</param>
        /// <returns>Index of the value or -1 if not found</returns>
        public int ResolveLocal(string name)
        {
            for (var i = Locals.Count - 1; i >= 0; i--)
            {
                if (Locals[i].Name == name)
                {
                    if (Locals[i].Depth == -1)
                        Parser.Error("Cannot read local variable in initializer.");

                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Resolves an upvalue
        /// </summary>
        /// <param name="name">Name of the upvalue</param>
        /// <returns>Index of the value or -1 if not found</returns>
        public int ResolveUpvalue(string name)
        {
            if (Enclosing is null) return -1;

            var local = Enclosing.ResolveLocal(name);
            if (local != -1)
            {
                Enclosing.Locals[local].IsCaptured = true;
                return AddUpvalue((byte)local, true);
            }

            var upvalue = Enclosing.ResolveUpvalue(name);
            if (upvalue != -1)
                return AddUpvalue((byte)upvalue, false);

            return -1;
        }

        /// <summary>
        /// Adds a new upvalue
        /// </summary>
        /// <param name="index">Index of the upvalue</param>
        /// <param name="isLocal">Locality of the upvalue</param>
        /// <returns>Index of the added upvalue, 0 if it cannot be handled</returns>
        public int AddUpvalue(byte index, bool isLocal)
        {
            foreach (var (value, i) in Upvalues.Select((value, i) => (value, i)))
            {
                if (value.Index == index && value.IsLocal == isLocal)
                    return i;
            }

            if (Upvalues.Count == Value.UINT8_COUNT)
            {
                Parser.Error($"Closures can handle at most {Value.UINT8_COUNT} variables.");
                return 0;
            }

            Upvalues.Add(new Upvalue(index, isLocal));
            var count = Upvalues.Count;
            Function.UpvalueCount = count;
            return count - 1;
        }

        /// <summary>
        /// Starts a new scope
        /// </summary>
        public void BeginScope()
        {
            ScopeDepth++;
        }

        /// <summary>
        /// Ends the open scope
        /// </summary>
        public void EndScope()
        {
            ScopeDepth--;

            while (Locals.Count != 0 && Locals.Last().Depth > ScopeDepth)
            {
                if (Locals.Last().IsCaptured)
                    Parser.Emit(Instruction.CloseUpv);
                else
                    Parser.Emit(Instruction.Pop);

                Locals.RemoveAt(Locals.Count - 1);
            }
        }

        /// <summary>
        /// Checks if the Resolver is working locally
        /// </summary>
        public bool IsLocal => ScopeDepth > 0;
    }
}
