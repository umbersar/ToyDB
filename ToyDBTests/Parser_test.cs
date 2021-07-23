using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyDB;

namespace ToyDBTests {
    [TestClass]
    public class Parser_test {
        struct Test_UseDB_Statements {
            public string input;
            public string expectedIdentifier;
            public object expectedValue;
        }
        [TestMethod]
        public void TestLetStatements() {

            var tests = new[]{
                                new Test_UseDB_Statements { input="use database testdb;", expectedIdentifier = "testdb", expectedValue="testdb"},
                              };

            foreach (var test in tests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                if (program == null) {
                    throw new AssertFailedException("ParseProgram() returned nil");
                }

                if (program.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
                }


                var stmt = program.Statements[0];
                if (!testLetStatement(stmt, test.expectedIdentifier)) { }

                AST_Helper.Expression value = ((LetStatement)stmt).Value;
                if (!testLiteralExpression(value, test.expectedValue)) { }
            }
        }
    }
}
