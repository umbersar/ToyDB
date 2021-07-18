using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDBTests {
    [TestClass]
    class AST_test {
        [TestMethod]
        public void Test_AST_ToString() {
            string inputString = @"let myVar = anotherVar;";

            //construct the AST by hand and then serialize it to spit out the string
            Program program = new Program() {
                Statements = new List<Statement>() {
                                    new LetStatement(){ Token = new Token(TokenHelper.TokenType.LET, "let"),
                                                        Name = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "myVar"), Value="myVar"},
                                                        Value = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "anotherVar"), Value="anotherVar"}
                                                      }
                }
            };

            string AST_ToString = program.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", program.ToString()));
        }
    }
}
