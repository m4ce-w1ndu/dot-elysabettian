using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Common
{
    /// <summary>
    /// Holds data to represent a Token in the language
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// Type of the token
        /// </summary>
        public TokenType Type { get; init; }
        
        /// <summary>
        /// Text value of the token
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Line bound to this token
        /// </summary>
        public int Line { get; init; }

        /// <summary>
        /// Constructs a new Token
        /// </summary>
        /// <param name="type">Type of the token</param>
        /// <param name="text">Text value of the token</param>
        /// <param name="line">Line bound to the token</param>
        public Token(TokenType type, string text, int line)
        {
            Type = type;
            Text = text;
            Line = line;
        }
    }
}
