using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ToyDB.AST_Helper;

namespace ToyDB {
    public class CreateTblStatement : Statement {
        public Token Token { get; set; }// the TokenType.CREATE token
        public Identifier TableName { get; set; }

        public List<Identifier> Columns = new List<Identifier>();//TODO: we are assuming that datatype would be int as that is what we store at the moment.
        //but the data type needs to be stored as well if we are to handle other column types

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral() + " ");
            stringBuilder.Append("table" + " ");//we injected this string into the AST.ToString() representation as AST does not need to reflect the code as is.
            if (this.TableName != null)//this assumes a LetStatement can be without a value. Does not make sense when "=" has been assumed to be present
                stringBuilder.Append(this.TableName.ToString());

            stringBuilder.Append("(");
            stringBuilder.Append(string.Join(",", Columns.Select(a => a + "")));
            stringBuilder.Append(")");

            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }
}
