using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public class Identifier : AST_Helper.Expression {
        public Token Token { get; set; }

        //Token encapsulates the Token type and string literal. Whereas Value here represents parsed value depending on token type. So Value field 
        //of Integer class would of type int and will hold the actual value
        public string Value { get; set; }

        public override void expressionNode() {

        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            return this.Value;//i could have also returned this.Token.Literal here.
        }
    }
}
