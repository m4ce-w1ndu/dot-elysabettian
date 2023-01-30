using Lexer.Common;

namespace UnitTests.Lexer
{
    public class Token_UTests
    {
        [Fact]
        public void Construct_TokenWithType()
        {
            var token = new Token(TokenType.If, "if", 1);

            Assert.Equal(TokenType.If, token.Type);
            Assert.Equal("if", token.Text);
            Assert.Equal(1, token.Line);
        }
    }
}
