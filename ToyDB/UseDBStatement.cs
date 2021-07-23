using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ToyDB.AST_Helper;

namespace ToyDB {
    public class UseDBStatement : Statement {
        public Token Token { get; set; }// the TokenType.USE token
        public Identifier DBName { get; set; }


        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral() + " ");
            if (this.DBName != null)//this assumes a UseStatement can be without a value. Does not make sense when "=" has been assumed to be present
                stringBuilder.Append(this.DBName.ToString());
            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }
}
