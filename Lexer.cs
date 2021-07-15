using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public class Lexer {
        string input;
        int position;//this is the current pos
        int readPosition;//this is next chars' position
        char ch;//char at 'position'

        public Lexer(string input) {
            this.input = input;
            this.readChar();
        }

        public void readChar() {
            if (this.readPosition >= input.Length) {
                this.ch = '\0';

                //setting the char to int 0 would have worked by typecasting it to char 
                //but setting it to C style NULL (string termination character) is more explicit IMO.
                //A binary null character is just a char with an integer/ASCII value of 0.
                //Console.WriteLine(Convert.ToChar(0x0).ToString());//The hexidecimal 0x0 is the null character  
                //Console.WriteLine('\0'.ToString());
                //Console.WriteLine(char.MinValue.ToString());
                //Console.WriteLine(Convert.ToChar(0).ToString());
            } else {
                this.ch = this.input[readPosition];
            }

            this.position = this.readPosition;
            this.readPosition++;
        }

        public char peekChar() {
            if (this.readPosition >= input.Length) {
                return (char)0;//this is equivalent to '\0'
            } else {
                return this.input[readPosition];
            }
        }

        public Token NextToken() {
            Token tok;
            this.skipWhiteSpace();
            switch (this.ch) {
               case ';':
                    tok = new Token(TokenHelper.TokenType.SEMICOLON, this.ch.ToString());
                    break;
                case ',':
                    tok = new Token(TokenHelper.TokenType.COMMA, this.ch.ToString());
                    break;
                case '\0':
                    tok = new Token(TokenHelper.TokenType.EOF, this.ch.ToString());//todo:??
                    break;
                default:
                    if (this.isLetter(this.ch)) {
                        string literal = this.readIdentifier();
                        TokenHelper.TokenType type = TokenHelper.LookupIdent(literal);

                        tok = new Token(type, literal);
                        //we don't want to call readchar() again(below) as we have already read the next char in readIdentifier. Therefore return here.
                        return tok;
                    } else {
                        tok = new Token(TokenHelper.TokenType.ILLEGAL, this.ch.ToString());
                    }
                    break;
            }

            this.readChar();
            return tok;
        }

        private string readIdentifier() {
            var temp_position = this.position;
            while (isLetter(this.ch)) {
                this.readChar();
            }
            return this.input.Substring(temp_position, (this.position - temp_position) /*+ 1*/);
        }

        private bool isLetter(char ch) {
            return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';
        }

        private void skipWhiteSpace() {
            while (this.ch == ' ' || this.ch == '\t' || this.ch == '\n' || this.ch == '\r') {
                this.readChar();
            }
        }
    }
}
