namespace Compiler.Common.Types
{
    /// <summary>
    /// Represents an instance of a class.
    /// </summary>
    public class Instance
    {
        /// <summary>
        /// Class that has been instantiated.
        /// </summary>
        public Class Class { get; set; }

        /// <summary>
        /// Fields of this instance.
        /// </summary>
        public Dictionary<string, Value> Fields { get; set; }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="class_">Class to instantiate.</param>
        public Instance(Class class_)
        {
            Class = class_;
            Fields = new();
        }
    }
}
