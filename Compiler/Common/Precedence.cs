namespace Compiler.Common
{
    /// <summary>
    /// Expresses the parsing precedence for every
    /// type of statement and expression.
    /// </summary>
    public enum Precedence
    {
        None,
        Assignment,
        Or,
        And,
        Equality,
        Comparison,
        Term,
        Factor,
        Unary,
        Call,
        Primary
    }
}