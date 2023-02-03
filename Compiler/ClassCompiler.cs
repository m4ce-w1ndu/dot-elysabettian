namespace Compiler
{
    /// <summary>
    /// Implementation of Class Compiler
    /// </summary>
    public class ClassCompiler
    {
        /// <summary>
        /// Internal class compiler
        /// </summary>
        public ClassCompiler? Enclosing { get; set; }

        /// <summary>
        /// Whether it has or it hasn't a superclass.
        /// </summary>
        public bool HasSuperclass { get; set; }

        /// <summary>
        /// Constructs a new ClassCompiler
        /// </summary>
        /// <param name="enclosing">Enclosing (internal) class compiler</param>
        public ClassCompiler(ClassCompiler? enclosing)
        {
            Enclosing = enclosing;
            HasSuperclass = false;
        }
    }
}
