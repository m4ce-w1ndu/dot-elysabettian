using System.Reflection;
using Lexer;
using Lexer.Common;

namespace UnitTests.Lexer
{
    public class Scanner_UTests
    {
        /// <summary>
        /// Checks if the construction of the scanner
        /// is completed correctly.
        /// </summary>
        [Fact]
        public void Construct_ScannerWithSource()
        {
            const string source = "var x = 5;";
            var scanner = new Scanner(source);

            var info = typeof(Scanner).GetField("source", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info is null) Assert.Fail("Cannot retrieve field.");

            var value = (string)info.GetValue(scanner)!;
            Assert.Equal(source, value);
        }

        /// <summary>
        /// Checks if the scanner is able to correctly
        /// tokenize a variable expression.
        /// </summary>
        [Fact]
        public void Tokenize_ScannerTokenizerVar()
        {
            const string source = "var x = 5;";
            var expectedTokens = new List<Token>()
            {
                new Token(TokenType.Var, "var", 1),
                new Token(TokenType.Identifier, "x", 1),
                new Token(TokenType.Equal, "=", 1),
                new Token(TokenType.Number, "5", 1),
                new Token(TokenType.Semicolon, ";", 1)
            };

            var scanner = new Scanner(source);

            var tokenList = new List<Token>();
            for (var token = scanner.ScanToken(); token.Type != TokenType.Eof; token = scanner.ScanToken())
                tokenList.Add(token);

            // List of tokenized source should be the same as
            // the one provided as literal.
            Assert.Equal(expectedTokens, tokenList);
        }

        /// <summary>
        /// This test checks if the Scanner is able to
        /// properly tokenize a function defined on multiple
        /// lines of code.
        /// </summary>
        [Fact]
        public void Tokenize_ScannerTokenizeFunc()
        {
            const string source =
@"  func square(x)
    {
        return (x * x);
    }
";

            // List of expected tokens
            var expectedTokens = new List<Token>()
            {
                new Token(TokenType.Func, "func", 1),
                new Token(TokenType.Identifier, "square", 1),
                new Token(TokenType.OpenParen, "(", 1),
                new Token(TokenType.Identifier, "x", 1),
                new Token(TokenType.CloseParen, ")", 1),
                new Token(TokenType.OpenCurly, "{", 2),
                new Token(TokenType.Return, "return", 3),
                new Token(TokenType.OpenParen, "(", 3),
                new Token(TokenType.Identifier, "x", 3),
                new Token(TokenType.Star, "*", 3),
                new Token(TokenType.Identifier, "x", 3),
                new Token(TokenType.CloseParen, ")", 3),
                new Token(TokenType.Semicolon, ";", 3),
                new Token(TokenType.CloseCurly, "}", 4)
            };
            var scanner = new Scanner(source);

            var tokens = new List<Token>();
            for (var token = scanner.ScanToken(); token.Type != TokenType.Eof; token = scanner.ScanToken())
                tokens.Add(token);

            Assert.Equal(expectedTokens, tokens);
        }
    }
}
