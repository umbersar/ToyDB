using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public static class AST_Helper {
        public abstract class Node {
            public abstract string TokenLiteral();

            public abstract override string ToString();//helper for printing the AST nodes for debugging 
        }

        public abstract class Statement : Node {
            public abstract void statementNode();
        }
        public abstract class Expression : Node {
            public abstract void expressionNode();
        }
    }
}
