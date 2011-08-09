using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpBot
{
    namespace Math
    {
        /// <summary>
        /// Provides basic math expression parsing functionality. 
        /// </summary>
        public static class Math
        {
            public const string ValidChars = "0123456789+-*/";
            public const string OperChars = "+-*/";

            public static string Parse(string expr) {
                // First we validate the expression. 
                if (expr.IndexOfAny(ValidChars.ToArray()) == -1) {
                    return "Invalid expression. ";
                }
                if (expr[0].EqualsAny(OperChars.ToArray()) ||
                    expr[expr.Length - 1].EqualsAny(OperChars.ToArray())) {
                    return "Invalid expression. ";
                }

                // Generate an Expression array and get the result out!
                return Expression.Result(expr).ToString();
            }
        }

        public enum Oper
        {
            None,
            Add,
            Subtract,
            Multiply,
            Divide,
        }

        /// <summary>
        /// An expression, such as '3', '3+3', or '3*3+3'. 
        /// </summary>
        public class Expression
        {
            public const int MultDivide = 2;
            public const int AddSubtract = 1;
            public const int None = 0;

            private readonly List<Term> _terms;

            public Expression(Term[] terms) {
                _terms = terms.ToList();
            }

            public double Value {
                get {
                    double retValue = 0;
                    while (HasOpers) {
                        for (int i = 0; i < _terms.Count; i++) {
                            if (_terms[i].Operator == Oper.None) continue; // If the working term isn't an oper
                            // don't continue. 
                            // Using 3 terms at a time, get their value and remove them from the working list. 
                            if (_terms[i].Operator == Oper.Multiply || _terms[i].Operator == Oper.Divide) {
                                if (_terms[i].Operator == Oper.Multiply) {
                                    retValue = _terms[i - 1].Value * _terms[i + 1].Value;
                                } else if (_terms[i].Operator == Oper.Divide) {
                                    retValue = _terms[i - 1].Value / _terms[i + 1].Value;
                                }
                                continueProc(retValue, i);
                            } else if ((_terms[i].Operator == Oper.Add || _terms[i].Operator == Oper.Subtract) &&
                                       HighestPrecedence <= AddSubtract) {
                                if (_terms[i].Operator == Oper.Add) {
                                    retValue = _terms[i - 1].Value + _terms[i + 1].Value;
                                } else if (_terms[i].Operator == Oper.Subtract) {
                                    retValue = _terms[i - 1].Value - _terms[i + 1].Value;
                                }
                                continueProc(retValue, i);
                            }
                        }
                    }
                    return retValue;
                }
            }

            public int HighestPrecedence {
                get {
                    int retVal = 0;
                    if (HasOpers) {
                        foreach (Term t in _terms) {
                            if (t.TermString.EqualsAny(new[] { "*", "/" })) {
                                retVal = MultDivide;
                            }
                            if (t.TermString.EqualsAny(new[] { "+", "-" })) {
                                retVal = AddSubtract;
                            }
                        }
                    }
                    return retVal;
                }
            }

            public bool HasOpers {
                get { return _terms.Any(t => t.Operator != Oper.None); }
            }

            private void continueProc(double retValue, int i) {
                _terms.Insert(i + 2, new Term(retValue.ToString()));
                _terms.RemoveRange(i - 1, 3);
            }

            public static double Result(string expr) {
                // Split expr by all oper characters and get the list of opers. 
                string[] termStrings = expr.Split(Math.OperChars.ToArray());
                List<Term> numericTerms = termStrings.Select(s => new Term(s)).ToList();
                List<Term> operTerms = getOperators(expr);

                // Merge the two lists. 
                List<Term> termsList = mergeTerms(numericTerms, operTerms);

                // Create the expression and get it's value. 
                var e = new Expression(termsList.ToArray());
                return e.Value;
            }

            private static List<Term> mergeTerms(List<Term> numericTerms, List<Term> operTerms) {
                // If we have no operators, our Terms list is already a simple expression and 
                // can be simply returned. 
                if (operTerms.Count == 0) {
                    return numericTerms;
                }

                var retValue = new List<Term>();
                // If we get here, we have actual work to do. 
                for (int i = 0; i < numericTerms.Count; i++) {
                    if (i < operTerms.Count) // We havn't run out of operators to merge. 
                    {
                        retValue.Add(numericTerms[i]);
                        retValue.Add(operTerms[i]);
                    } else // We have no more operators to merge. 
                    {
                        retValue.Add(numericTerms[i]);
                    }
                }
                return retValue;
            }

            private static List<Term> getOperators(string expr) {
                var operTerms = new List<Term>();
                foreach (char c in expr) {
                    if (c.EqualsAny(Math.OperChars.ToArray())) {
                        operTerms.Add(new Term(c.ToString()));
                    }
                }
                return operTerms;
            }
        }

        /// <summary>
        /// An expression term, such as '+', '33', or '*'. 
        /// </summary>
        public class Term
        {
            public readonly string TermString;

            public Term(string term) {
                TermString = term;
            }

            public Oper Operator {
                get {
                    switch (TermString) {
                        case "+":
                            return Oper.Add;
                        case "-":
                            return Oper.Subtract;
                        case "*":
                            return Oper.Multiply;
                        case "/":
                            return Oper.Divide;
                        default:
                            return Oper.None;
                    }
                }
            }

            /// <summary>
            /// Gets this Term's value. Do not call on an operatior term, or you'll get an 
            /// ArgumentException.
            /// </summary>
            public double Value {
                get {
                    if (Operator == Oper.None) {
                        return Double.Parse(TermString);
                    }
                    throw new ArgumentException("This Term is an operator term.");
                }
            }
        }
    }
}