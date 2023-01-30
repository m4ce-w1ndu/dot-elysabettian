using Lexer.Common;

namespace UnitTests.Lexer
{
    public class Token_UTests
    {
        /// <summary>
        /// This test checks that the parametrized constuctor
        /// actually performs its work. This test is useless,
        /// but it is written to demonstrate that xUnit is
        /// properly configured.
        /// </summary>
        [Fact]
        public void Construct_TokenWithType()
        {
            var token = new Token(TokenType.If, "if", 1);

            Assert.Equal(TokenType.If, token.Type);
            Assert.Equal("if", token.Text);
            Assert.Equal(1, token.Line);
        }

        /// <summary>
        /// This test checks that the default values are
        /// effectively confirming an undefined behaviour.
        /// </summary>
        [Fact]
        public void Construct_Empty()
        {
            var token = new Token();

            Assert.Equal(TokenType.OpenParen, token.Type);
            Assert.Null(token.Text);
            Assert.Equal(0, token.Line);
        }
    }
}
