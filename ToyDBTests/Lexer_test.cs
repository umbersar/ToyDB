using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToyDB;

namespace ToyDBTests {
    [TestClass]
    public class Lexer_test {
        struct Test_Token {
            public int tokenNumber;
            public TokenHelper.TokenType expectedType;
            public string expectedLiteral;
        }

        [TestMethod]
        public void TestNextToken_EOF() {
            string inputString = "";

            var tests = new[] { new { tokenNumber=0, expectedType = TokenHelper.TokenType.EOF, expectedLiteral = '\0'.ToString() }
                                };

            var lex = new Lexer(inputString);
            foreach (var test in tests) {
                var tok = lex.NextToken();
                if (tok.Type != test.expectedType) {
                    throw new System.Exception(string.Format("wrong tokentype. expected={0}, got={1}", test.expectedType, tok.Type));
                }

                if (tok.Literal != test.expectedLiteral) {
                    throw new System.Exception(string.Format("wrong literal. expected={0}, got={1}", test.expectedLiteral, tok.Literal));
                }
            }
        }

        [TestMethod]
        public void TestNextToken_keywords_funcs() {
            string inputString = @"create database testdb;
                                   use database testdb;
                                   create table testtbl(cola int, colb int);
                                   insert into testtbl values(1,2);
                                   select cola, colb from testtbl;
                                ";


            var tests = new[] { new Test_Token{tokenNumber=0, expectedType = TokenHelper.TokenType.CREATE, expectedLiteral = "create" },
                                new Test_Token{tokenNumber=1, expectedType = TokenHelper.TokenType.DATABASE, expectedLiteral = "database" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "testdb" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{tokenNumber=0, expectedType = TokenHelper.TokenType.USE, expectedLiteral = "use" },
                                new Test_Token{tokenNumber=1, expectedType = TokenHelper.TokenType.DATABASE, expectedLiteral = "database" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "testdb" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{tokenNumber=0, expectedType = TokenHelper.TokenType.CREATE, expectedLiteral = "create" },
                                new Test_Token{tokenNumber=1, expectedType = TokenHelper.TokenType.TABLE, expectedLiteral = "table" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "testtbl" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.LPAREN, expectedLiteral = "(" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "cola" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.INT, expectedLiteral = "int" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "colb" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.INT, expectedLiteral = "int" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.RPAREN, expectedLiteral = ")" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.INSERT, expectedLiteral = "insert" },
                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.INTO, expectedLiteral = "into" },
                                new Test_Token{ tokenNumber=7, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "testtbl" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.VALUES, expectedLiteral = "values" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.LPAREN, expectedLiteral = "(" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "1" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "2" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.RPAREN, expectedLiteral = ")" },
                                new Test_Token{ tokenNumber=9, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.SELECT, expectedLiteral = "select" },
                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "cola" },
                                new Test_Token{ tokenNumber=7, expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "colb" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.FROM, expectedLiteral = "from" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "testtbl" },
                                new Test_Token{ tokenNumber=9, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
            };

            TestTokens(inputString, tests);
        }

        private static void TestTokens(string inputString, Test_Token[] tests) {
            var lex = new Lexer(inputString);

            int index = -1;
            foreach (var test in tests) {
                index++;
                var tok = lex.NextToken();
                if (tok.Type != test.expectedType) {
                    throw new AssertFailedException(string.Format("token number {0} - wrong tokentype. expected={1}, got={2}", index, test.expectedType, tok.Type));
                }

                if (tok.Literal != test.expectedLiteral) {
                    throw new AssertFailedException(string.Format("token number {0} - wrong literal. expected={1}, got={2}", index, test.expectedLiteral, tok.Literal));
                }
            }
        }
    }
}
