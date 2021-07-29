using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyDB {
    public static class Evaluator {
        //whereas the AST that is input to the Evaluator might contains nulls(.net null), the output of Evaluator would only contain null_AST.
        //for e.g., the alternative of a IfExpression might have .net null but that has to be evaluated to null_AST.
        public static readonly Null_AST null_AST = new Null_AST();

        public static Object_AST Eval(AST_Helper.Node node, Environment_AST env) {
            switch (node) {//makes use of pattern matching. otherwise would have to use if else 
                case Program p:
                    return EvalProgram(p.Statements, env);//logic for Program and BlockStatement is similar but varies in the case where the BlockStatement is 
                                                          //actually a nested block having a return in the nested(deeper/inner) level

                case BlockStatement blkStmt://would handle, for e.g., consequence and alternative statement blocks in IfExpression 
                    return EvalBlockStatement(blkStmt.Statements, env);

                case ExpressionStatement stmt:
                    return Eval(stmt.Expression, env);

                case FunctionExpression funcExpr:
                    return new Function_AST(funcExpr.Parameters, funcExpr.Body, env);

                case HashLiteral hash:
                    return evalHashLiteral(hash, env);

                case FunctionCallExpression funcCallExpr:
                    Object_AST func = Eval(funcCallExpr.Function, env);//Function could be Identifier(case below) or FunctionExpression(case above)
                    if (func is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return func;       //higher level error obscuring the true source of error
                    }

                    List<Object_AST> args = evalExpressions(funcCallExpr.Arguments, env);//Arguments is just a list of Expression
                    if (args.Count == 1 && args[0] is Error_AST) {
                        return args[0];
                    }

                    //now that we have Eval 'ed the Function(Identifier or FunctionExpression) as well as the arguments, why do't we just Eval the Body which is just a BlockStatement.
                    //Before we Eval the Body, we have to provide it its own Enviroment (for binding the above eval'ed arguments to the function parameters)

                    return applyFunction(func, args);

                case ArrayIndexExpression arrIdxExpr:
                    Object_AST left = Eval(arrIdxExpr.Left, env);
                    if (left is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return left;       //higher level error obscuring the true source of error
                    }

                    Object_AST index = Eval(arrIdxExpr.Index, env);
                    if (index is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return left;       //higher level error obscuring the true source of error
                    }

                    return evalIndexExpression(left, index);

                case LetStatement letStmt:
                    Object_AST val = Eval(letStmt.Value, env);
                    if (val is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return val;       //higher level error obscuring the true source of error
                    }

                    env.Set(letStmt.Name.Value, val);
                    return val;

                case ArrayLiteral arr:
                    List<Object_AST> elements = evalExpressions(arr.Elements, env);
                    if (elements.Count == 1 && elements[0] is Error_AST) {
                        return elements[0];
                    }

                    return new Array_AST { Elements = elements };

                case Identifier ident:
                    return evalIdentifier(ident, env);

                case PrefixExpression prefixExpression:
                    Object_AST rightExpr = Eval(prefixExpression.Right, env);
                    if (rightExpr is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return rightExpr;       //higher level error obscuring the true source of error
                    }

                    return evalPrefixExpression(prefixExpression.Operator, rightExpr);

                case InfixExpression infixExpression:
                    Object_AST leftExpr = Eval(infixExpression.Left, env);
                    if (leftExpr is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return leftExpr;        //higher level error obscuring the true source of error
                    }
                    Object_AST right = Eval(infixExpression.Right, env);
                    if (right is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return right;       //higher level error obscuring the true source of error
                    }

                    return evalInfixExpression(infixExpression.Operator, leftExpr, right);

                case ReturnStatement retStmt://we evaluate the expression associated with ReturnStatement and wrap that in a ReturnValue_AST object to be used later
                    Object_AST retVal = Eval(retStmt.ReturnValue, env);//so if we are returning a int, that int is first evaluated and wrapped in a Integer_AST
                    if (retVal is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                        return retVal;         //higher level error obscuring the true source of error
                    }

                    return new ReturnValue_AST(retVal);//and then that Integer_AST is again wrapped inside a ReturnValue_AST.

                case IfExpression ifExpr:
                    return evalIfExpression(ifExpr, env);

                case IntegerLiteral i:
                    return new Integer_AST(i.Value);

                case BooleanLiteral i:
                    //return new Boolean_AST(i.Value);// this will create new objects for every true or false encountered. stop this profileration 
                    return nativeBoolToBooleanObject(i.Value);

                case StringLiteral i:
                    return new String_AST(i.Value);

                default:
                    //return new Null_AST();// this will create new null objects every time. stop this profileration 
                    return null_AST;

                case null:
                    throw new ArgumentNullException(nameof(node));
            }
        }

        private static Object_AST evalHashLiteral(HashLiteral hash, Environment_AST env) {
            Dictionary<HashKey, HashPair> pairs = new Dictionary<HashKey, HashPair>();
            foreach (var item in hash.Pairs) {
                Object_AST key = Eval(item.Key, env);
                if (key is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                    return key;       //higher level error obscuring the true source of error
                }

                IHashable hashKey = key as IHashable;
                if (hashKey is null) {
                    return new Error_AST(string.Format("unusable as hash key: {0}", key.Type));
                }

                Object_AST val = Eval(item.Value, env);
                if (val is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                    return val;       //higher level error obscuring the true source of error
                }

                HashKey hashed = hashKey.HashKey();
                pairs[hashed] = new HashPair { Key = key, Value = val };
            }

            return new Hash_AST { Pairs = pairs };
        }

        private static Object_AST evalIndexExpression(Object_AST left, Object_AST index) {
            switch (left, index) {
                case (Array_AST arr, Integer_AST i):
                    return evalArrayIndexExpression(left, index);
                case (Hash_AST hash, _):
                    return evalHashIndexExpression(left, index);
                default:
                    return new Error_AST(string.Format("index operator not supported: {0}", left.Type));
            }
            throw new NotImplementedException();
        }

        private static Object_AST evalHashIndexExpression(Object_AST hash, Object_AST index) {
            Hash_AST hash_AST = hash as Hash_AST;

            IHashable key = index as IHashable;
            if (key is null) {
                return new Error_AST(string.Format("unusable as hash key: {0}", index.Type));
            }

            if (hash_AST.Pairs.TryGetValue(key.HashKey(), out HashPair pair))
                return pair.Value;
            else
                return null_AST;

            throw new NotImplementedException();
        }

        private static Object_AST evalArrayIndexExpression(Object_AST arr, Object_AST index) {
            Array_AST array_AST = arr as Array_AST;
            long idx = (index as Integer_AST).Value;
            int max = array_AST.Elements.Count - 1;

            if (idx < 0 || idx > max) {
                return null_AST;
            }

            return array_AST.Elements[(int)idx];
        }

        private static Object_AST applyFunction(Object_AST func, List<Object_AST> args) {
            switch (func) {
                case Function_AST function:
                    Environment_AST extendedEnv = extendFunctionEnv(function, args);
                    Object_AST evaluated = Eval(function.Body, extendedEnv);//function.Body is a BlockStatement. Hence EvalBlockStatement is executed. 

                    return unwrapReturnValue(evaluated);
                case BuiltinFunc_AST builtinFunc:
                    return builtinFunc.fn(args.ToArray());//we don't unwrap for builtin func as we never return 'ReturnValue_AST' from them. We intend to return other basic 
                                                          //builtin/monkey types such as Integer_AST, String_AST, Boolean_AST and Null_AST without wrapping them.
                                                          //todo: why do we not wrap builtin return values in ReturnValue_AST? 
                default:
                    return new Error_AST(string.Format("not a function: {0}", func.Type));
            }
            //Function_AST function = func as Function_AST;
            //if (function is null) {
            //    return new Error_AST(string.Format("not a function: {0}", func.Type));
            //}

            //Environment_AST extendedEnv = extendFunctionEnv(function, args);
            //Object_AST evaluated = Eval(function.Body, extendedEnv);//function.Body is a BlockStatement. Hence EvalBlockStatement is executed. 

            //return unwrapReturnValue(evaluated);
        }

        //todo: why are we unwrapping the value here. I would have assumed we would just keep using the wrapped "ReturnValue_AST" while we are walking the AST just like we do in 
        //EvalBlockStatement and only unwrap it in EvalProgram. Therefore the end user guarenteed to only ever see the actual unwrapped return value as the entry point is EvalProgram.
        //One reason could have been that EvalBlockStatement does not need to unwrap it but we need to unwrap it in the case of a function call is because we don't bind return value of BlockStatement
        //to a identifier. And thus we need the need unwrapped value in case of function call. But i could have used BlockStatement to pass arguments to function parameters. Thus in that case we do
        //need unwrapped value of BlockStatement to bind to Function parameter:
        //Example: myfunc(if(10>1){ if(10>1) {return 10;} return 1;}) Here i am calling a function myfunc and passing arguments in the form of a BlockStatement. If BlockStatement return values are not unwrapped
        //, then how do bind the value to the parameter??

        //The reason of unwrapping as given in book is "That’s necessary, because otherwise a return statement would bubble up through several functions and stop the evaluation in all of 
        //them.But we only want to stop the evaluation of the last called function’s body.That’s why we need unwrap it, so that evalBlockStatement won’t stop evaluating statements in “outer” functions."
        //So, if i understand correctly, is that both Functions and BlockStatements can be nested. But if we are in nested BlockStatement, a return in a deep BlockStatement would break out of the whole hierarchy(so
        //that means if the nested BlockStatements are in a function, control would be returned from the outer scope function). In the case of nested Functions, a return from a inner function will only return 
        //the control to outer scope function(jumps 1 nesting level)

        //In other words, a return in a nested block of statements in the same exeution context(in the same function or at the same level in call stack) should cause a return to a level up in 
        //the call hierachy(calling function) but for nested functions cause a return to a outer scope function. 

        private static Object_AST unwrapReturnValue(Object_AST retVal) {
            ReturnValue_AST retVal_AST = retVal as ReturnValue_AST;
            if (retVal_AST is null) {
                return retVal;
            } else
                return retVal_AST.Value;//unwrap
        }

        //bind the arguments passed to the function parameters in a Environment (specific to the function). The outer enviroment contains the outer scope of function (thus we can handle free variables captured
        //capture by closure as well as global variables). Note that the Environment reference in the Function_AST class is to outer scope/enviroment of a function and in this method we create a enclosed environment which will
        //hold the argument-parameter bindings for the function call.
        private static Environment_AST extendFunctionEnv(Function_AST function, List<Object_AST> args) {
            Environment_AST env = Environment_AST.NewEnclosedEnvironment_AST(function.Env);

            for (int index = 0; index < function.Parameters.Count; index++) {
                var param = function.Parameters[index];
                env.Set(param.Value, args[index]);
            }

            return env;
        }

        private static List<Object_AST> evalExpressions(List<AST_Helper.Expression> expressions, Environment_AST env) {
            List<Object_AST> result = new List<Object_AST>();

            foreach (var exp in expressions) {
                Object_AST evaluated = Eval(exp, env);//we are Evaluating the arguments the func in left-to-right order
                if (evaluated is Error_AST) {
                    return new List<Object_AST>() { evaluated };//if error, construct a new empty list(could have purged the existing one as well), add the error and return.          
                }

                result.Add(evaluated);
            }

            return result;
        }

        private static Object_AST evalIdentifier(Identifier ident, Environment_AST env) {
            (Object_AST val, bool found) = env.Get(ident.Value);
            if (found)
                return val;
            else if (BuiltInFuncDefs.builtinFuncs.TryGetValue(ident.Value, out BuiltinFunc_AST func))
                return func;
            else
                return new Error_AST(string.Format("identifier not found: {0}", ident.Value));
        }

        private static Object_AST evalIfExpression(IfExpression ifExpr, Environment_AST env) {
            Object_AST condition = Eval(ifExpr.Condition, env);
            if (condition is Error_AST) {//for recursive Eval calls, check the returned result of the recursive Eval. If an error, return rightaway. Otherwise we will get a 
                return condition;         //higher level error obscuring the true source of error
            }

            if (isTruthy(condition)) {
                return Eval(ifExpr.Consequence, env);
            } else if (ifExpr.Alternative != null) {//you can expect .net null in AST but in the Evaluator they have to be changed to null_AST (monkey's null)
                return Eval(ifExpr.Alternative, env);
            } else
                return null_AST;
        }

        //this would be called after our evaluator has evaluated the expresison we used for if condition.
        private static bool isTruthy(Object_AST condition) {
            switch (condition) {

                case Null_AST n://todo:what condition would be evaluated by our Evaluator to be a null_AST?? If i comment this case, all the 'TestIfElseExpressions' tests pass
                    return false;

                //if we used an expression in the condition that evaluated to a bool (which evaluator would have converted to our own representation of bool), 
                //then handle that
                case Boolean_AST boolean_AST:

                    if (boolean_AST == true_AST) {
                        return true;
                    } else return false;

                default://if we used ints for the condition(for e.g. if(3) {10} ), the treat the condition as true
                    return true;
            }

        }

        private static Object_AST evalInfixExpression(string Operator, Object_AST leftExpr, Object_AST rightExpr) {
            if (leftExpr.Type == ObjectType_AST.Integer_AST && rightExpr.Type == ObjectType_AST.Integer_AST) {
                return evalIntegerInfixExpression(Operator, leftExpr, rightExpr);
            } else if (leftExpr.Type == ObjectType_AST.String_AST && rightExpr.Type == ObjectType_AST.String_AST) {
                return evalStringInfixExpression(Operator, leftExpr, rightExpr);
            } else if (Operator == "==") {
                return nativeBoolToBooleanObject(leftExpr == rightExpr);//we support only int and bool. so this should handle bool operands. we can do reference check for equality
                //as the bools are represented by only 2 hardcoded objects in our system. We do not need unwrap them to check for values inside them as we do with ints.
            } else if (Operator == "!=") {
                return nativeBoolToBooleanObject(leftExpr != rightExpr);
            } else if (leftExpr.Type != rightExpr.Type) {
                return new Error_AST(string.Format("type mismatch: {0} {1} {2}", leftExpr.Type, Operator, rightExpr.Type));
            } else {
                //return null_AST;//todo:find a way to throw errors with meaningful messages
                return new Error_AST(string.Format("unknown operator: {0} {1} {2}", leftExpr.Type, Operator, rightExpr.Type));
            }

        }

        private static Object_AST evalIntegerInfixExpression(string Operator, Object_AST left, Object_AST right) {
            long leftVal = (left as Integer_AST).Value;
            long rightVal = (right as Integer_AST).Value;

            switch (Operator) {
                case "+":
                    return new Integer_AST(leftVal + rightVal);
                case "-":
                    return new Integer_AST(leftVal - rightVal);
                case "*":
                    return new Integer_AST(leftVal * rightVal);
                case "/":
                    return new Integer_AST(leftVal / rightVal);

                case "<":
                    return nativeBoolToBooleanObject(leftVal < rightVal);
                case ">":
                    return nativeBoolToBooleanObject(leftVal > rightVal);
                case "==":
                    return nativeBoolToBooleanObject(leftVal == rightVal);
                case "!=":
                    return nativeBoolToBooleanObject(leftVal != rightVal);

                default:
                    //return null_AST;//todo: placeholder for error handling
                    return new Error_AST(string.Format("unknown operator: {0} {1} {2}", left.Type, Operator, right.Type));
            }
        }

        private static Object_AST evalStringInfixExpression(string Operator, Object_AST left, Object_AST right) {
            string leftVal = (left as String_AST).Value;
            string rightVal = (right as String_AST).Value;

            switch (Operator) {
                case "+":
                    return new String_AST(leftVal + rightVal);

                default:
                    //return null_AST;//todo: placeholder for error handling
                    return new Error_AST(string.Format("unknown operator: {0} {1} {2}", left.Type, Operator, right.Type));
            }
        }

        private static Object_AST evalPrefixExpression(string Operator, Object_AST rightExpr) {
            switch (Operator) {
                case "!":
                    return evalBangOperatorExpression(rightExpr);
                case "-":
                    return evalMinusPrefixOperatorExpression(rightExpr);
                default:
                    //return null_AST;//todo: placeholder for error handling
                    return new Error_AST(string.Format("unknown operator: {0} {1}", Operator, rightExpr.Type));
            }
        }

        private static Object_AST evalMinusPrefixOperatorExpression(Object_AST rightExpr) {
            switch (rightExpr) {//makes use of pattern matching. otherwise would have to use if-else statements.
                case Integer_AST i:
                    return new Integer_AST(-i.Value);
                default:
                    //return null_AST;//todo: placeholder for error handling
                    return new Error_AST(string.Format("unknown operator: -{0}", rightExpr.Type));
            }
        }

        private static Object_AST evalBangOperatorExpression(Object_AST rightExpr) {
            if (rightExpr == true_AST) {
                return false_AST;
            } else if (rightExpr == false_AST) {
                return true_AST;
            } else if (rightExpr == null_AST) {
                return true_AST;
            } else
                return false_AST;
        }

        private static Object_AST nativeBoolToBooleanObject(bool value) {
            if (value)
                return true_AST;
            else
                return false_AST;
        }

        private static Object_AST EvalProgram(List<AST_Helper.Statement> statements, Environment_AST env) {
            Object_AST result = new Null_AST();
            foreach (var stmt in statements) {
                result = Eval(stmt, env);

                //pattern matching
                switch (result) {
                    case ReturnValue_AST returnValue_AST://if we had not used ReturnValue_AST wrapper for the 'return' value, we could have not have known that we now have to stop execution of statements that follow. 
                        return returnValue_AST.Value;//ReturnValue_AST is used internally in the interpreter while walking the AST. the end user would only see the actual wrapped result

                    case Error_AST error:
                        return result;//stop the execution of statements to follow in the case of an error result as well.

                    default:
                        break;
                }
            }

            return result; //Note that the result of Eval of last statement in the Program(or BlockStatement) is returned if we had not used a ReturnStatement 
        }

        private static Object_AST EvalBlockStatement(List<AST_Helper.Statement> statements, Environment_AST env) {
            Object_AST result = new Null_AST();
            foreach (var stmt in statements) {
                result = Eval(stmt, env);

                //pattern matching
                switch (result) {//todo:code in book checks for null here which i do not think is necessary.
                    case ReturnValue_AST returnValue_AST:
                        return returnValue_AST;//this is the difference in logic between current method and EvalProgram (above). This handles a 'return' deep in a nested BlockStatement hierarchy 
                                               //and 'breaks' out of it but does not unwrap the ReturnValue_AST (unwrapping is done in EvalProgram)
                    case Error_AST error:
                        return result;//stop the execution of statements to follow in the case of an error result as well.

                    default:
                        break;
                }
            }

            return result;
        }
    }
}
