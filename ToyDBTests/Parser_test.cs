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
        }
        [TestMethod]
        public void TestUseDBStatements() {

            var tests = new[]{
                                new Test_UseDB_Statements { input="use testdb;", expectedIdentifier = "testdb"},
                              };

            foreach (var test in tests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var sqlBatch = p.ParseSQLBatch();
                checkParserErrors(p);

                if (sqlBatch == null) {
                    throw new AssertFailedException("ParseProgram() returned nil");
                }

                if (sqlBatch.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", sqlBatch.Statements.Count));
                }


                var stmt = sqlBatch.Statements[0];
                if (!testUseDBStatement(stmt, test.expectedIdentifier)) { }
            }
        }

        private bool testUseDBStatement(AST_Helper.Statement s, string name) {
            if (s.TokenLiteral() != "use") {
                throw new AssertFailedException(string.Format("s.TokenLiteral not 'use'. got ={0}", s.TokenLiteral()));
            }

            UseDBStatement useDBStmnt;
            try {
                useDBStmnt = (UseDBStatement)s;
            } catch (Exception) {
                throw new AssertFailedException(string.Format("Expected UseDBStatement. got={0}", s));
            }

            if (useDBStmnt.DBName.Value != name) {
                throw new AssertFailedException(string.Format("useDBStmnt.DBName.Value not '{0}'. got={1}", name, useDBStmnt.DBName.Value));
            }

            if (useDBStmnt.DBName.TokenLiteral() != name) {
                throw new AssertFailedException(string.Format("useDBStmnt.DBName.TokenLiteral() not '{0}'. got={1}",
                                                                name, useDBStmnt.DBName.TokenLiteral()));
            }

            return true;
        }

        private void checkParserErrors(Parser p) {
            var errors = p.Errors();
            if (errors.Count == 0) return;

            var errorMessage = string.Format("parser has {0} errors", errors.Count);
            foreach (var error in errors) {
                errorMessage = errorMessage + Environment.NewLine + string.Format("parser error: {0}", error);
            }

            throw new AssertFailedException(errorMessage);
        }
    }
}
