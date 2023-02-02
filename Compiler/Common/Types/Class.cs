namespace Compiler.Common.Types
{
    /// <summary>
    /// Represents a class in the language.
    /// </summary>
    public class Class
    {
        /// <summary>
        /// Name of the class.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Methods in the class.
        /// </summary>
        public Dictionary<string, Closure> Methods { get; init; }

        /// <summary>
        /// Constructs a new class.
        /// </summary>
        /// <param name="name">Name of the class.</param>
        public Class(string name)
        {
            Name = name;
            Methods = new();
        }
    }
}