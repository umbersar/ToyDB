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

        struct Test_CreateDB_Statements {
            public string input;
            public string expectedIdentifier;
        }

        struct Test_CreateTbl_Statements {
            public string input;
            public string expectedIdentifier;
            public List<string> expectedColumns;
        }
        struct Test_InsertTbl_Statements {
            public string input;
            public string expectedIdentifier;
            public List<int> expectedValues;
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

        [TestMethod]
        public void TestCreateDBStatements() {

            var tests = new[]{
                                new Test_CreateDB_Statements { input="create database testdb;", expectedIdentifier = "testdb"},
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
                if (!testCreateDBStatement(stmt, test.expectedIdentifier)) { }
            }
        }

        private bool testCreateDBStatement(AST_Helper.Statement s, string name) {
            if (s.TokenLiteral() != "create") {
                throw new AssertFailedException(string.Format("s.TokenLiteral not 'create'. got ={0}", s.TokenLiteral()));
            }

            CreateDBStatement createDBStmnt;
            try {
                createDBStmnt = (CreateDBStatement)s;
            } catch (Exception) {
                throw new AssertFailedException(string.Format("Expected CreateDBStatement. got={0}", s));
            }

            if (createDBStmnt.DBName.Value != name) {
                throw new AssertFailedException(string.Format("createDBStmnt.DBName.Value not '{0}'. got={1}", name, createDBStmnt.DBName.Value));
            }

            if (createDBStmnt.DBName.TokenLiteral() != name) {
                throw new AssertFailedException(string.Format("createDBStmnt.DBName.TokenLiteral() not '{0}'. got={1}",
                                                                name, createDBStmnt.DBName.TokenLiteral()));
            }

            return true;
        }

        [TestMethod]
        public void TestCreateTblStatements() {

            var tests = new[]{
                                new Test_CreateTbl_Statements { input="create table testtbl(cola,colb);", expectedIdentifier = "testtbl", expectedColumns = new List<string>(){"cola", "colb" } },
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
                if (!testCreateTblStatement(stmt, test.expectedIdentifier, test.expectedColumns)) { }
            }
        }

        private bool testCreateTblStatement(AST_Helper.Statement s, string name, List<string> expectedColumns) {
            if (s.TokenLiteral() != "create") {
                throw new AssertFailedException(string.Format("s.TokenLiteral not 'create'. got ={0}", s.TokenLiteral()));
            }

            CreateTblStatement createTblStmnt;
            try {
                createTblStmnt = (CreateTblStatement)s;
            } catch (Exception) {
                throw new AssertFailedException(string.Format("Expected CreateTblStatement. got={0}", s));
            }

            if (createTblStmnt.TableName.Value != name) {
                throw new AssertFailedException(string.Format("createTblStmnt.TableName.Value not '{0}'. got={1}", name, createTblStmnt.TableName.Value));
            }

            if (createTblStmnt.TableName.TokenLiteral() != name) {
                throw new AssertFailedException(string.Format("createTblStmnt.TableName.TokenLiteral() not '{0}'. got={1}",
                                                                name, createTblStmnt.TableName.TokenLiteral()));
            }

            var columns = createTblStmnt.Columns.Select(i => i.TokenLiteral()).ToList();
            if (!columns.SequenceEqual(expectedColumns)) {
                throw new AssertFailedException(string.Format("createTblStmnt.Columns not '{0}'. got={1}",
                                                                string.Join(",", expectedColumns), string.Join(",", columns)));
            }


            return true;
        }

        [TestMethod]
        public void TestinsertTblStatements() {

            var tests = new[]{
                                new Test_InsertTbl_Statements { input="insert into testtbl values(11,3);", expectedIdentifier = "testtbl", expectedValues = new List<int>(){ 11, 3} },
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
                if (!testInsertTblStatement(stmt, test.expectedIdentifier, test.expectedValues)) { }
            }
        }

        private bool testInsertTblStatement(AST_Helper.Statement s, string name, List<int> expectedValues) {
            if (s.TokenLiteral() != "create") {
                throw new AssertFailedException(string.Format("s.TokenLiteral not 'create'. got ={0}", s.TokenLiteral()));
            }

            InsertTblStatement insertTblStmnt;
            try {
                insertTblStmnt = (InsertTblStatement)s;
            } catch (Exception) {
                throw new AssertFailedException(string.Format("Expected InsertTblStatement. got={0}", s));
            }

            if (insertTblStmnt.TableName.Value != name) {
                throw new AssertFailedException(string.Format("createTblStmnt.TableName.Value not '{0}'. got={1}", name, insertTblStmnt.TableName.Value));
            }

            if (insertTblStmnt.TableName.TokenLiteral() != name) {
                throw new AssertFailedException(string.Format("insertTblStmnt.TableName.TokenLiteral() not '{0}'. got={1}",
                                                                name, insertTblStmnt.TableName.TokenLiteral()));
            }

            var columns = insertTblStmnt.DataToInsert.Select(i => int.Parse(i.TokenLiteral())).ToList();
            if (!columns.SequenceEqual(expectedValues)) {
                throw new AssertFailedException(string.Format("insertTblStmnt.TableName not '{0}'. got={1}",
                                                                string.Join(",", expectedValues), string.Join(",", columns)));
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
