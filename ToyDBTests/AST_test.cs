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
            SQLBatch sqlBatch = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new CreateDBStatement(){ Token = new Token(TokenHelper.TokenType.CREATE, "create"),
                                                        //the token database is not stored in AST for the create statement as it is assumed to be there
                                                        DBName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testdb"), Value="testdb"}
                                                      }
                }
            };

            string AST_ToString = sqlBatch.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", sqlBatch.ToString()));
        }

        [TestMethod]
        public void Test_Use_AST_ToString() {
            string inputString = @"use testdb;";

            //construct the AST by hand and then serialize it to spit out the string
            SQLBatch sqlBatch = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new UseStatement(){ Token = new Token(TokenHelper.TokenType.USE, "use"),
                                                        DBName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testdb"), Value="testdb"}
                                                      }
                }
            };

            string AST_ToString = sqlBatch.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", sqlBatch.ToString()));
        }

        [TestMethod]
        public void Test_CreateTbl_AST_ToString() {
            string inputString = @"create table testtbl(cola,colb);";//data types for columns are assumed to be int. 

            //construct the AST by hand and then serialize it to spit out the string
            SQLBatch sqlBatch = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new CreateTblStatement(){ Token = new Token(TokenHelper.TokenType.CREATE, "create"),
                                                             TableName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testtbl"), Value="testtbl"},
                                                             Columns = new List<Identifier>(){
                                                                                                new Identifier() { Token= new Token(TokenHelper.TokenType.IDENT, "cola")
                                                                                                                    , Value="cola"
                                                                                                                 },
                                                                                                new Identifier() { Token= new Token(TokenHelper.TokenType.IDENT, "colb")
                                                                                                                    , Value="colb"
                                                                                                                 }
                                                                                             }
                                                      }
                }
            };

            string AST_ToString = sqlBatch.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", sqlBatch.ToString()));
        }

        [TestMethod]
        public void Test_InsertIntoTbl_AST_ToString() {
            string inputString = @"insert into testtbl values(11,3);";//data types for columns are assumed to be int. 

            //construct the AST by hand and then serialize it to spit out the string
            SQLBatch sqlBatch = new SQLBatch() {
                Statements = new List<Statement>() {
                                    new InsertTblStatement(){ Token = new Token(TokenHelper.TokenType.INSERT, "insert"),
                                                             TableName = new Identifier(){ Token= new Token(TokenHelper.TokenType.IDENT, "testtbl"), Value="testtbl"},
                                                             DataToInsert = new List<Expression>(){
                                                                                                new IntegerLiteral() { Token= new Token(TokenHelper.TokenType.INT, "11")
                                                                                                                    , Value=11
                                                                                                                 },
                                                                                                new IntegerLiteral() { Token= new Token(TokenHelper.TokenType.INT, "3")
                                                                                                                    , Value=3
                                                                                                                 }
                                                                                             }
                                                      }
                }
            };

            string AST_ToString = sqlBatch.ToString();
            if (AST_ToString != inputString)
                throw new AssertFailedException(string.Format("program.String() wrong. got={0}", sqlBatch.ToString()));
        }
    }
}
