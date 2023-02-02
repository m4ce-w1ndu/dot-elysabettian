namespace Compiler.Common.Types
{
    /// <summary>
    /// Holds reference to a native environment function.
    /// </summary>
    public struct NativeFunction
    {
        /// <summary>
        /// Pointer to function.
        /// </summary>
        public Func<Value, int, Value> Function { get; init; }

        public NativeFunction(Func<Value, int, Value> function)
        {
            Function = function;
        }
    }
}