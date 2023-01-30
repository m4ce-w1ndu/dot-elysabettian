using Lexer.Common;
using System.Diagnostics.Contracts;

namespace Lexer
{
    /// <summary>
    /// Scanner is able to tokenize a source string
    /// into a set of valid language tokens.
    /// </summary>
    public class Scanner
    {
        /// <summary>
        /// Holds the source code.
        /// </summary>
        private readonly string source;

        /// <summary>
        /// Holds the start position of the scanner.
        /// </summary>
        private int start;

        /// <summary>
        /// Holds the current position of the scanner.
        /// </summary>
        private int current;

        /// <summary>
        /// Holds the current line of code the scanner is
        /// operating on.
        /// </summary>
        private int line;

        /// <summary>
        /// Creates a new Scanner.
        /// </summary>
        /// <param name="source">Source code to tokenize.</param>
        public Scanner(string source)
        {
            this.source = source;
            start = 0;
            current = 0;
            line = 1;
        }

        /// <summary>
        /// Tokenizes the source code.
        /// </summary>
        /// <returns>The next valid Token in the source code.</returns>
        public Token ScanToken()
        {
            SkipWhitespace();
            start = current;
            if (IsAtEnd()) return MakeToken(TokenType.Eof);

            var c = Advance();
            if (char.IsDigit(c)) return Number();
            if (char.IsLetter(c)) return Identifier();

            return c switch
            {
                '(' => MakeToken(TokenType.OpenParen),
                ')' => MakeToken(TokenType.CloseParen),
                '[' => MakeToken(TokenType.OpenSquare),
                ']' => MakeToken(TokenType.CloseSquare),
                '{' => MakeToken(TokenType.OpenCurly),
                '}' => MakeToken(TokenType.CloseCurly),
                ';' => MakeToken(TokenType.Semicolon),
                ',' => MakeToken(TokenType.Comma),
                '.' => MakeToken(TokenType.Dot),
                '^' => MakeToken(TokenType.Caret),
                '~' => MakeToken(TokenType.Tilde),
                '+' => Match('=') ? MakeToken(TokenType.PlusEqual) : MakeToken(TokenType.Plus),
                '-' => Match('=') ? MakeToken(TokenType.MinusEqual) : MakeToken(TokenType.Minus),
                '/' => Match('=') ? MakeToken(TokenType.SlashEqual) : MakeToken(TokenType.Slash),
                '*' => Match('=') ? MakeToken(TokenType.StarEqual) : MakeToken(TokenType.Star),
                '&' => Match('&') ? MakeToken(TokenType.AmpAmp) : MakeToken(TokenType.Amp),
                '|' => Match('|') ? MakeToken(TokenType.PipePipe) : MakeToken(TokenType.Pipe),
                '!' => Match('=') ? MakeToken(TokenType.ExclEqual) : MakeToken(TokenType.Excl),
                '=' => Match('=') ? MakeToken(TokenType.EqualEqual) : MakeToken(TokenType.Equal),
                '<' => Match('=') ? MakeToken(TokenType.LessEqual) : MakeToken(TokenType.Less),
                '>' => Match('=') ? MakeToken(TokenType.GreaterEqual) : MakeToken(TokenType.Greater),
                '"' => String(),
                '\'' => String('\''),
                _ => ErrorToken("Unexpected character in input.")
            };
        }

        /// <summary>
        /// Checks if the Scanner is at the end of input.
        /// </summary>
        /// <returns>
        /// True if the scanner is at the end of input,
        /// false otherwise.
        /// </returns>
        private bool IsAtEnd()
        {
            return current == source.Length;
        }

        /// <summary>
        /// Advances the scanner.
        /// </summary>
        /// <returns>Character at current position.</returns>
        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        /// <summary>
        /// Peeks the current character.
        /// </summary>
        /// <returns>Character at current position.</returns>
        [Pure]
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        /// <summary>
        /// Peeks at the next character.
        /// </summary>
        /// <returns>
        /// Character at next position or null terminator
        /// if scanner reached the end-of-input.
        /// </returns>
        [Pure]
        private char PeekNext()
        {
            if ((current + 1) >= source.Length) return '\0';
            return source[current + 1];
        }

        /// <summary>
        /// Matches the current character and advances scanner.
        /// </summary>
        /// <param name="expected">Expected character to match.</param>
        /// <returns>
        /// True if the character matches, false otherwise.
        /// </returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        /// <summary>
        /// Creates a new Token with the given type.
        /// </summary>
        /// <param name="type">Type of the token.</param>
        /// <returns>Created token.</returns>
        [Pure]
        private Token MakeToken(TokenType type)
        {
            var text = source[start..current];
            return new Token(type, text, line);
        }

        /// <summary>
        /// Creates a new Token holding an error message.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <returns>Created token.</returns>
        [Pure]
        private Token ErrorToken(string message)
        {
            return new Token(TokenType.Error, message, line);
        }

        /// <summary>
        /// Skips all white spaces in the source code.
        /// </summary>
        private void SkipWhitespace()
        {
            while (true)
            {
                var c = Peek();
                switch (c)
                {
                    case ' ':
                    case '\r':
                    case '\t':
                        Advance();
                        break;

                    case '\n':
                        line++;
                        Advance();
                        break;

                    case '/':
                        if (PeekNext() == '/') return;
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                        break;

                    default: return;
                }
            }
        }

        /// <summary>
        /// Checks the given "rest" of keyword agains the
        /// source code.
        /// </summary>
        /// <param name="pos">Position to start checking.</param>
        /// <param name="len">Length of substring.</param>
        /// <param name="rest">Value of rest substring.</param>
        /// <param name="type">Type of the token.</param>
        /// <returns>The correct type of the given substring.</returns>
        [Pure]
        private TokenType CheckKeyword(int pos, int len, string rest, TokenType type)
        {
            if (current - start == pos + len && source.Substring(start + pos, len).Equals(rest))
                return type;

            return TokenType.Identifier;
        }

        /// <summary>
        /// Checks if a word is a language keyword or an identifier.
        /// </summary>
        /// <returns>Type of the word (keyword or ident)</returns>
        [Pure]
        private TokenType IdentifierType()
        {
            return source[start] switch
            {
                'c' => CheckKeyword(1, 4, "lass", TokenType.Class),
                'e' => CheckKeyword(1, 3, "lse", TokenType.Else),
                'f' => current - start <= 1 ? TokenType.Identifier : source[start + 1] switch
                {
                    'a' => CheckKeyword(2, 3, "lse", TokenType.False),
                    'o' => CheckKeyword(2, 1, "r", TokenType.For),
                    'u' => CheckKeyword(2, 2, "nc", TokenType.Func),
                    _ => TokenType.Identifier,
                },
                'i' => CheckKeyword(1, 1, "f", TokenType.If),
                'n' => CheckKeyword(1, 3, "ull", TokenType.Null),
                'p' => CheckKeyword(1, 4, "rint", TokenType.Print),
                'r' => CheckKeyword(1, 5, "eturn", TokenType.Return),
                's' => CheckKeyword(1, 4, "uper", TokenType.Super),
                't' => current - start <= 1 ? TokenType.Identifier : source[start + 1] switch
                {
                    'h' => CheckKeyword(2, 2, "is", TokenType.This),
                    'r' => CheckKeyword(2, 2, "ue", TokenType.True),
                    _ => TokenType.Identifier,
                },
                'v' => CheckKeyword(1, 2, "ar", TokenType.Var),
                'w' => CheckKeyword(1, 4, "hile", TokenType.While),
                _ => TokenType.Identifier
            };
        }

        /// <summary>
        /// Creates a new identifier Token.
        /// </summary>
        /// <returns>Token holding an identifier.</returns>
        private Token Identifier()
        {
            while (char.IsLetterOrDigit(Peek())) Advance();
            return MakeToken(IdentifierType());
        }

        /// <summary>
        /// Creates a new Number token.
        /// </summary>
        /// <returns>Token holding a number.</returns>
        private Token Number()
        {
            while (char.IsDigit(Peek())) Advance();

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(Peek())) Advance();
            }

            return MakeToken(TokenType.Number);
        }

        /// <summary>
        /// Creates a new String token.
        /// </summary>
        /// <param name="delimiter">String open/close delimiter.</param>
        /// <returns>Token holding a string literal.</returns>
        private Token String(char delimiter = '"')
        {
            while (Peek() != delimiter && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd()) return ErrorToken("Unterminated string literal.");

            Advance();
            return MakeToken(TokenType.String);
        }
    }
}
