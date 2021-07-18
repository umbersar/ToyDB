using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public class Environment_AST {
        private Dictionary<string, Object_AST> _store;
        private Environment_AST _outer;//used to keep track of nested scopes

        private Environment_AST() {
            this._store = new Dictionary<string, Object_AST>();
            this._outer = null;
        }

        public static Environment_AST NewEnvironment_AST() {
            return new Environment_AST();
        }

        public static Environment_AST NewEnclosedEnvironment_AST(Environment_AST outer) {
            //Environment_AST env = new Environment_AST() { _outer = outer};
            //env._outer = outer;

            return new Environment_AST() { _outer = outer };
        }

        public (Object_AST, bool) Get(string name) {
            bool found = _store.TryGetValue(name, out Object_AST value);
            if (!found && this._outer != null) {
                return this._outer.Get(name);
            }

            return (value, found);
        }

        public Object_AST Set(string name, Object_AST value) {
            _store[name] = value;
            return value;
        }
    }

    public interface Object_AST {
        public ObjectType_AST Type { get; }
        public string Inspect();
    }

    public enum ObjectType_AST {//Evaluation of our language by interpretor would result in one of these types. Some are used internally for walking the AST like (ReturnValue_AST) and never returned end user
        Integer_AST,
        Boolean_AST,
        ReturnValue_AST,
        Error_AST,
        Function_AST,
        String_AST,
        BuiltinFunc_AST,
        Array_AST,
        Hash_AST,

        Null_AST //Why do we have null if we do not have support for it in tokenizer?
    }
}
