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

        public SQLBatch ParseSQLBatch() {
            SQLBatch sqlBatch = new SQLBatch();

            while (!this.curTokenIs(TokenHelper.TokenType.EOF)) {
                var stmt = this.parseStatement();
                if (stmt != null) {
                    sqlBatch.Statements.Add(stmt);
                }
                this.nextToken();
            }

            return sqlBatch;
        }

        private AST_Helper.Statement parseStatement() {
            switch (this.curToken.Type) {
                case TokenHelper.TokenType.USE:
                    return this.parseUseDBStatement();
                    
                case TokenHelper.TokenType.CREATE:
                    if (this.peekTokenIs(TokenHelper.TokenType.DATABASE)) {
                        return this.parseCreateDBStatement();
                    } 

                    return this.parseCreateTblStatement();//we assume if it is not a CREATE DATABASE stmt then it must be CREATE TABLE stmt
                
                case TokenHelper.TokenType.INSERT:
                    return this.parseInsertTblStatement();
                
                default:
                    return this.parseExpressionStatement();
            }
        }

        private AST_Helper.Statement parseExpressionStatement() {
            throw new NotImplementedException();
        }

        private InsertTblStatement parseInsertTblStatement() {
            var stmt = new InsertTblStatement() { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.INTO)) {
                return null;
            }

            if (!this.expectPeek(TokenHelper.TokenType.TABLE)) {
                return null;
            }

            if (!this.expectPeek(TokenHelper.TokenType.IDENT)) {
                return null;
            }
            
            stmt.TableName = new Identifier() { Token = this.curToken, Value = this.curToken.Literal };

            if (!this.expectPeek(TokenHelper.TokenType.VALUES)) {
                return null;
            }

            if (!this.expectPeek(TokenHelper.TokenType.LPAREN)) {
                return null;
            }

            stmt.DataToInsert = this.parseInsertTableValues();

            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();
            }

            return stmt;
        }

        private List<Identifier> parseInsertTableValues() {
            List<Identifier> identifiers = new List<Identifier>();

            if (this.peekTokenIs(TokenHelper.TokenType.RPAREN)) {
                this.nextToken();
                return identifiers;
            }

            this.nextToken();
            Identifier ident = new Identifier { Token = this.curToken, Value = this.curToken.Literal };

            identifiers.Add(ident);

            while (this.peekTokenIs(TokenHelper.TokenType.COMMA)) {
                this.nextToken();
                this.nextToken();

                ident = new Identifier { Token = this.curToken, Value = this.curToken.Literal };
                identifiers.Add(ident);
            }

            if (!this.expectPeek(TokenHelper.TokenType.RPAREN)) {
                return null;
            }

            return identifiers;
        }


        private UseDBStatement parseUseDBStatement() {
            var stmt = new UseDBStatement() { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.IDENT)) {
                return null;
            }

            stmt.DBName = new Identifier() { Token = this.curToken, Value = this.curToken.Literal };

            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();
            }

            return stmt;
        }
        
        private CreateDBStatement parseCreateDBStatement() {
            var stmt = new CreateDBStatement() { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.DATABASE)) {
                return null;
            }

            if (!this.expectPeek(TokenHelper.TokenType.IDENT)) {
                return null;
            }

            stmt.DBName = new Identifier() { Token = this.curToken, Value = this.curToken.Literal };

            #region MyRegion
            ////TODO: skip expressions until we encounter a semicolon but replace this with correct parsing logic later
            //while (!this.curTokenIs(TokenHelper.TokenType.SEMICOLON)) {
            //    this.nextToken();
            //} 
            #endregion

            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();
            }

            return stmt;
        }

        private CreateTblStatement parseCreateTblStatement() {
            var stmt = new CreateTblStatement() { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.TABLE)) {
                return null;
            }

            if (!this.expectPeek(TokenHelper.TokenType.IDENT)) {
                return null;
            }

            stmt.TableName = new Identifier() { Token = this.curToken, Value = this.curToken.Literal };

            if (!this.expectPeek(TokenHelper.TokenType.LPAREN)) {
                return null;
            }

            stmt.Columns = this.parseTableColumns();

            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();
            }

            return stmt;
        }

        private List<Identifier> parseTableColumns() {
            List<Identifier> identifiers = new List<Identifier>();

            if (this.peekTokenIs(TokenHelper.TokenType.RPAREN)) {
                this.nextToken();
                return identifiers;
            }

            this.nextToken();
            Identifier ident = new Identifier { Token = this.curToken, Value = this.curToken.Literal };

            identifiers.Add(ident);

            while (this.peekTokenIs(TokenHelper.TokenType.COMMA)) {
                this.nextToken();
                this.nextToken();

                ident = new Identifier { Token = this.curToken, Value = this.curToken.Literal };
                identifiers.Add(ident);
            }

            if (!this.expectPeek(TokenHelper.TokenType.RPAREN)) {
                return null;
            }

            return identifiers;
        }

        private bool curTokenIs(TokenHelper.TokenType t) {
            return this.curToken.Type == t;
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

        Parser_Helper.precedence peekPrecedence() {
            if (Parser_Helper.precedences.ContainsKey(this.peekToken.Type))
                return Parser_Helper.precedences[this.peekToken.Type];
            else
                return Parser_Helper.precedence.LOWEST;
        }

        void registerPrefix(TokenHelper.TokenType t, Parser_Helper.prefixParserFn fn) {
            this.prefixParseFns[t] = fn;
        }
        public List<string> Errors() {
            return this.errors;
        }
    }
}
