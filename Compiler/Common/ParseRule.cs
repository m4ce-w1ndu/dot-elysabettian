namespace Compiler.Common
{
    /// <summary>
    /// Holds references to functions and precedence value
    /// useful for parsing phase.
    /// </summary>
    public struct ParseRule
    {
        /// <summary>
        /// Prefix parsing function.
        /// </summary>
        /// <value>Parsing function for prefix.</value>
        public Action<bool>? Prefix { get; init; }

        /// <summary>
        /// Infix parsing function.
        /// </summary>
        /// <value>Parsing function for infix.</value>
        public Action<bool>? Infix { get; init; }

        /// <summary>
        /// Parsing precedence enumeration.
        /// </summary>
        /// <value>Precedence of parsing phase.</value>
        public Precedence Precedence { get; init; }

        /// <summary>
        /// Constructs a new ParseRule
        /// </summary>
        /// <param name="prefix">Prefix rule function.</param>
        /// <param name="infix">Infix rule function.</param>
        /// <param name="precedence">Parsing precedence value.</param>
        public ParseRule(Action<bool>? prefix, Action<bool>? infix, Precedence precedence)
        {
            Prefix = prefix;
            Infix = infix;
            Precedence = precedence;
        }
    }
}