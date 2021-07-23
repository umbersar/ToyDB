using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public static class Parser_Helper {
        //instead of definning my own delegates here, i could have used readymade Action and Func delegate types. Func delegate is useful in this case as 
        //it has a retruntype
        //Func<AST_Helper.Expression>; which would be equivalent to prefixParserFn
        //Func<AST_Helper.Expression,AST_Helper.Expression>; which would be equivalent to infixParseFn
        //but when we make custom delegate types, we give them names and that makes the intention clearer
        //some info on delegates:https://www.pluralsight.com/guides/how-why-to-use-delegates-csharp

        public delegate AST_Helper.Expression prefixParserFn();
        public delegate AST_Helper.Expression infixParseFn(AST_Helper.Expression expression);

        //define operator predence. The operators here are not using the same names as equivalent operators in TokenType enum
        //for e.g., "==" is called EQ in TokenType enum and here it is called EQUALS. These just define precedence levels
        //and these levels are then associated with TokenTypes from TokenType enum in the dictionary below.
        public enum precedence {//todo:should have been named precedenceLevels(from lowest to highest)
            LOWEST,
            CALL, // myFunction(X)
        }

        //todo:should have been named operatorPrecedenceLevels
        public static Dictionary<TokenHelper.TokenType, precedence> precedences = new Dictionary<TokenHelper.TokenType, precedence> {

                                                        //2nd highest precedence. provides correct 'stickiness' to LPAREN to parse the functions parameters so 
                                                        //that they do not become infix operators when parseExpression is called
                                                        //todo: how CALL precedence for LPAREN not needed for correctly parsing the FunctionExpression arguments but is needed for
                                                        //FunctionCallEpxression parameters??
                                                        {TokenHelper.TokenType.LPAREN, precedence.CALL},

                                                };
    }

    public class Parser {
        Lexer l;
        Token curToken;
        Token peekToken;

        List<string> errors;
        //todo:prefixParseFns and infixParseFns are used in in parsing expression statements (one of the 3 statement types). But not sure about 
        //their usage. It seems, prefixParseFns does not have anything to do with prefix operators(??) or prefixExpression(the expressions which use
        //prefix operators like -5; or 10 + -5; or !foobar;). It seems that prefixParseFns contains prefix operators, IDENTIFIERS(like foobar) 
        //and LITERALs(like 5) with which an Expression(or an ExpressionStatement) can start and thus it includes prefix operators plus other 'stuff'.
        //prefixParseFns are being used to get the parsing correct and not to be confused with prefix operators or prefixExpression.
        Dictionary<TokenHelper.TokenType, Parser_Helper.prefixParserFn> prefixParseFns;
        Dictionary<TokenHelper.TokenType, Parser_Helper.infixParseFn> infixParseFns;
        public Parser(Lexer lexer) {
            this.l = lexer;
            errors = new List<string>();

            //the first expression in the expression statement is taken to be the prefix expression. And if there are more expressions to follow
            //this prefix expression is then joined to them as the leftExpr with rightExpr being the following expressions
            //
            this.prefixParseFns = new Dictionary<TokenHelper.TokenType, Parser_Helper.prefixParserFn>();
            this.registerPrefix(TokenHelper.TokenType.LPAREN, this.parseGroupedExpression);//for expressions which specify explicit precedence rules using parenthesis

            this.nextToken();
            this.nextToken();
        }
        private void nextToken() {
            this.curToken = this.peekToken;
            this.peekToken = this.l.NextToken();
        }

        private bool expectPeek(TokenHelper.TokenType t) {//if the expected token is present, this also advances the cursor to next token
            if (this.peekTokenIs(t)) {                    //and this should be reflected in the func name.
                this.nextToken();
                return true;
            } else {
                this.peekError(t);
                return false;
            }
        }

        private bool peekTokenIs(TokenHelper.TokenType t) {
            return this.peekToken.Type == t;
        }

        void peekError(TokenHelper.TokenType t) {
            string message = string.Format("expected next token to be {0}, got {1} instead", t, this.peekToken.Type);
            this.errors.Add(message);
        }

        private AST_Helper.Expression parseGroupedExpression() {
            this.nextToken();

            AST_Helper.Expression expression = this.parseExpression(Parser_Helper.precedence.LOWEST);

            if (!this.expectPeek(TokenHelper.TokenType.RPAREN)) {
                return null;
            }

            return expression;
        }

        private void noPrefixParseFnError(TokenHelper.TokenType type) {
            string message = string.Format("no prefix parse function for {0} found", type);
            this.errors.Add(message);
        }

        //this implements pratt parsing(top down recursive parsing)
        //DaBeaz's course implemented this with loop instead of recursion (so although top down, it is not recursive). Look in "Solution Items/stuff" folder
        //for snapshots showing his approach. Not only is it not recursive(he uses loops), but he has also created a func for each precendence level.
        //his approach is more readable than what we have here.
        private AST_Helper.Expression parseExpression(Parser_Helper.precedence precedence) {

            if (!this.prefixParseFns.ContainsKey(this.curToken.Type)) {
                noPrefixParseFnError(this.curToken.Type);
                return null;
            }
            Parser_Helper.prefixParserFn prefixParserFn = this.prefixParseFns[this.curToken.Type];

            AST_Helper.Expression leftExpr = prefixParserFn();

            while (!this.peekTokenIs(TokenHelper.TokenType.SEMICOLON) && precedence < this.peekPrecedence()) {
                if (!this.infixParseFns.ContainsKey(this.peekToken.Type)) {
                    return leftExpr;
                }

                Parser_Helper.infixParseFn infixParseFn = this.infixParseFns[this.peekToken.Type];
                this.nextToken();
                leftExpr = infixParseFn(leftExpr);//construct a new leftExpr expression using current leftExpr and next expr as rightExpr
            }

            return leftExpr;
        }

        void registerPrefix(TokenHelper.TokenType t, Parser_Helper.prefixParserFn fn) {
            this.prefixParseFns[t] = fn;
        }
    }
}
