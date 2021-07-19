using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyDB;
using static ToyDB.AST_Helper;

namespace ToyDBTests {
    [TestClass]
    public class AST_test {

        [TestMethod]
        public void Test_Create_AST_ToString() {
            string inputString = @"create database testdb;";

            //construct the AST by hand and then serialize it to spit out the string
            SQLBatch program = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new CreateDBStatement(){ Token = new Token(TokenHelper.TokenType.CREATE, "create"),
                                                        //the token database is not stored in AST for the create statement as it is assumed to be there
                                                        DBName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testdb"), Value="testdb"}
                                                      }
                }
            };

            string AST_ToString = program.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", program.ToString()));
        }

        [TestMethod]
        public void Test_Use_AST_ToString() {
            string inputString = @"use testdb;";

            //construct the AST by hand and then serialize it to spit out the string
            SQLBatch program = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new UseStatement(){ Token = new Token(TokenHelper.TokenType.USE, "use"),
                                                        DBName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testdb"), Value="testdb"}
                                                      }
                }
            };

            string AST_ToString = program.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", program.ToString()));
        }

        [TestMethod]
        public void Test_CreateTbl_AST_ToString() {
            Assert.Fail();
            string inputString = @"create table testtbl(cola int, colb int);";

            //construct the AST by hand and then serialize it to spit out the string
            SQLBatch program = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new UseStatement(){ Token = new Token(TokenHelper.TokenType.USE, "create"),
                                                        DBName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testdb"), Value="testdb"}
                                                      }
                }
            };

            string AST_ToString = program.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", program.ToString()));
        }
    }
}
