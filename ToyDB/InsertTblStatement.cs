using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ToyDB.AST_Helper;

namespace ToyDB {
    public class InsertTblStatement : Statement {
        public Token Token { get; set; }// the TokenType.INSERT token
        public Identifier TableName { get; set; }

        //TODO: change it to List<List<Expression>> to insert multiples rows at once
        public List<Expression> DataToInsert = new List<Expression>();

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral() + " ");
            stringBuilder.Append("into" + " ");//we injected this string into the AST.ToString() representation as AST does not need to reflect the code as is.
            if (this.TableName != null)//this assumes a LetStatement can be without a value. Does not make sense when "=" has been assumed to be present
                stringBuilder.Append(this.TableName.ToString() + " ");

            stringBuilder.Append("values");//we injected this string into the AST.ToString() representation as AST does not need to reflect the code as is.
            stringBuilder.Append("(");
            stringBuilder.Append(string.Join(",", DataToInsert.Select(a => a + "")));
            stringBuilder.Append(")");

            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }

}
