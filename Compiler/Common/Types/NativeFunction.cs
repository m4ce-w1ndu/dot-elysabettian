namespace Compiler.Common.Types
{
    /// <summary>
    /// Holds reference to a native environment function.
    /// </summary>
    public class NativeFunction
    {
        /// <summary>
        /// Pointer to function.
        /// </summary>
        public Func<int, IEnumerable<Value>, Value> Function { get; init; }

        /// <summary>
        /// Constructs a new NativeFunction.
        /// </summary>
        /// <param name="function">NativeFunction reference (delegate)</param>
        public NativeFunction(Func<int, IEnumerable<Value>, Value> function)
        {
            Function = function;
        }
    }
}