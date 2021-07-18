using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ToyDB.AST_Helper;

namespace ToyDB {

    public class CreateStatement : Statement {
        public Token Token { get; set; }// the TokenType.CREATE token
        public Identifier Name { get; set; }
        public Expression Value { get; set; }

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral() + " ");
            stringBuilder.Append(this.Name.ToString());
            stringBuilder.Append(" = ");
            if (this.Value != null)//this assumes a LetStatement can be without a value. Does not make sense when "=" has been assumed to be present
                stringBuilder.Append(this.Value.ToString());
            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }

}
