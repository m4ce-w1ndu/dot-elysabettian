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
        public Func<Value, int, Value> Function { get; init; }

        /// <summary>
        /// Constructs a new NativeFunction.
        /// </summary>
        /// <param name="function">NativeFunction reference (delegate)</param>
        public NativeFunction(Func<Value, int, Value> function)
        {
            Function = function;
        }
    }
}