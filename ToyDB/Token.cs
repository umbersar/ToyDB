using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public class Token {
        public TokenHelper.TokenType Type { get; private set; }
        public string Literal { get; private set; }

        public Token(TokenHelper.TokenType type, string literal) { Type = type; Literal = literal; }

        public override string ToString() {
            return "TokenType=" + Type + " : Literal:" + Literal;
        }

    }
}
