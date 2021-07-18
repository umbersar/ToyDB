using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public static class REPL {
        const string prompt = ">";
        const string ToyDB = @"                 
  _____           ____  ____   
 |_   _|__  _   _|  _ \| __ )  
   | |/ _ \| | | | | | |  _ \  
   | | (_) | |_| | |_| | |_) | 
   |_|\___/ \__, |____/|____/  
            |___/              
        ";

        public static void Start(TextReader tIn, TextWriter tOut) {

            //create the env variable in outer scope so that it persists after executing a line of code and we can use a IDENT later
            Environment_AST env = Environment_AST.NewEnvironment_AST();

            while (true) {
                tOut.Write(prompt);

                string line = tIn.ReadLine();
                if (string.IsNullOrEmpty(line)) break;

                Lexer lex = new Lexer(line);
                #region MyRegion
                //var tok = lex.NextToken();
                //while (tok.Type != TokenHelper.TokenType.EOF) {
                //    tOut.WriteLine(tok);
                //    tok = lex.NextToken();
                //}
                #endregion
                //Parser p = new Parser(lex);

                //Program program = p.ParseProgram();
                //if (p.Errors().Count != 0) {
                //    printParserError(tOut, p.Errors());
                //    continue;
                //}

                ////tOut.WriteLine(program.ToString());
                ////tOut.WriteLine();
                //Object_AST evaluated = Evaluator.Eval(program, env);
                //if (evaluated != null) {
                //    tOut.WriteLine(evaluated.Inspect());
                //}
            }
        }

        private static void printParserError(TextWriter tOut, List<string> errors) {
            tOut.WriteLine(ToyDB);
            tOut.WriteLine("Woops! We ran into some monkey business here!");
            tOut.WriteLine(" parser errors:");
            foreach (var msg in errors) {
                tOut.WriteLine("\t" + msg);
            }
        }
    }
}
