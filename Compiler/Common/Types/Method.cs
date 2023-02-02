namespace Compiler.Common.Types
{
    /// <summary>
    /// Represents a method.
    /// </summary>
    public struct Method
    {
        /// <summary>
        /// Instance of the class bound with this method.
        /// </summary>
        public Instance Receiver { get; set; }

        /// <summary>
        /// Instance of the function.
        /// </summary>
        public Closure Function { get; set; }

        /// <summary>
        /// Constructs a new Method.
        /// </summary>
        /// <param name="receiver">Instance of the class to bind.</param>
        /// <param name="function">Instance of the function.</param>
        public Method(Instance receiver, Closure function)
        {
            Receiver = receiver;
            Function = function;
        }
    }
}
