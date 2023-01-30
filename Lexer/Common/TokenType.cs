namespace Lexer.Common
{
    /// <summary>
    /// Identifies the type of a unique Token
    /// </summary>
    public enum TokenType
    {
        // Single-character tokens.
        OpenParen, CloseParen,
        OpenCurly, CloseCurly,
        OpenSquare, CloseSquare,
        Comma, Dot, Semicolon,
        Tilde, Caret,

        // One or two character tokens.
        Plus, PlusEqual, Minus, MinusEqual,
        Star, StarEqual, Slash, SlashEqual,
        Excl, ExclEqual, Equal, EqualEqual,
        Greater, GreaterEqual, Less, LessEqual,
        Amp, AmpAmp, Pipe, PipePipe,

        // Literals.
        Identifier, String, Number,

        // Keywords.
        Class, Else, False, Func,
        For, If, Null, Print, Return,
        Super, This, True, Var, While,

        Error,
        Eof,
    }
}
