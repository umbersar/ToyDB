using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ToyDB.AST_Helper;

namespace ToyDB {
    public class InfixExpression : AST_Helper.Expression {
        public Token Token { get; set; }
        public Expression Left { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }

        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("(");
            stringBuilder.Append(this.Left.ToString());
            stringBuilder.Append(" " + this.Operator + " ");
            stringBuilder.Append(this.Right.ToString());
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}
