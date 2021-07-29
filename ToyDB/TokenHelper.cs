using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public static class TokenHelper {

        //i could have used default int enum but specifying the 'char' values (not string) help in readability. 
        //That char value is internally converted to int. And for that reason can't use strings enum values. 
        //Should not use chars for some of the enum entries as thi bug illustrates:
        //https://stackoverflow.com/questions/63806156/wrong-enum-value-in-c-sharp
        public enum TokenType {
            ILLEGAL,
            EOF,

            IDENT,

            PLUS,
            MINUS,

            COMMA,
            SEMICOLON,

            LPAREN,//(
            RPAREN,//)

            SELECT,
            FROM,
            USE,
            CREATE,
            DATABASE,
            TABLE,
            INT,
            INSERT,
            INTO,
            VALUES
        }


        //keywords is a mapping dictionary which maps string literals to TokenTypes.
        static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> {
                                                        {"select", TokenType.SELECT},
                                                        {"from", TokenType.FROM},
                                                        {"use", TokenType.USE},
                                                        {"create", TokenType.CREATE},
                                                        {"database", TokenType.DATABASE},
                                                        {"table", TokenType.TABLE},
                                                        {"int", TokenType.INT},
                                                        {"insert", TokenType.INSERT},
                                                        {"into", TokenType.INTO},
                                                        {"values", TokenType.VALUES }
                                                };

        public static TokenType LookupIdent(string ident) {
            //use ident.ToLower() to handle case-insensitive match
            ident = ident.ToLower();
            if (keywords.ContainsKey(ident))//is it a keyword like SELECT
            {
                return keywords[ident];
            } else
                return TokenType.IDENT;//else it is a table or column or database name literal
        }
    }
}
