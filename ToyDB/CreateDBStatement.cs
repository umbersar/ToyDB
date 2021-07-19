using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ToyDB.AST_Helper;

namespace ToyDB {

    public class CreateDBStatement : Statement {
        public Token Token { get; set; }// the TokenType.CREATE token
        public Identifier DBName { get; set; }
        

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral() + " ");
            stringBuilder.Append("database" + " ");//we injected this string into the AST.ToString() representation as AST does not need to reflect the code as is.
            if (this.DBName != null)//this assumes a LetStatement can be without a value. Does not make sense when "=" has been assumed to be present
                stringBuilder.Append(this.DBName.ToString());
            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }

}
