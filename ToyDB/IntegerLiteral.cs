using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public class IntegerLiteral : AST_Helper.Expression {
        public Token Token { get; set; }

        //Token encapsulates the Token type and string literal. Whereas Value here represents parsed value depending on token type. So Value field 
        //of Integer class would of type int and will hold the actual value
        public int Value { get; set; }

        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            return this.Value.ToString();//this would be same a returning this.Token.Literal in this case
        }
    }
}
