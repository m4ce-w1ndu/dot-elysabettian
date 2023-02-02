namespace Compiler.Common.Types
{
    /// <summary>
    /// Implementation of an array type.
    /// </summary>
    public class Array
    {
        /// <summary>
        /// List of values.
        /// </summary>
        public List<Value> Values;

        /// <summary>
        /// Number of values.
        /// </summary>
        public int Count => Values.Count;

        /// <summary>
        /// Constructs a new Value.
        /// </summary>
        public Array()
        {
            Values = new();
        }
    }
}
