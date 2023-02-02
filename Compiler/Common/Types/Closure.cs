using Compiler.Utilities;

namespace Compiler.Common.Types
{
    /// <summary>
    /// Represents a closure.
    /// </summary>
    public struct Closure
    {
        /// <summary>
        /// Function associated to this closure.
        /// </summary>
        public Function Function { get; set; }

        /// <summary>
        /// Upvalues in this closure.
        /// </summary>
        public List<Upvalue?> Upvalues { get; set; }

        /// <summary>
        /// Constructs a new Closure
        /// </summary>
        /// <param name="function">Function of this closure.</param>
        public Closure(Function function)
        {
            Function = function;
            Upvalues = new();
            Upvalues.Resize(function.UpvalueCount, null);
        }
    }
}
