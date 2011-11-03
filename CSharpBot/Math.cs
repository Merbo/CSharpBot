using System;
using System.Collections.Generic;
using System.Linq;
using CSharpBot;

namespace CSharpBot
{
    /// <summary>
    /// Provides basic math expression parsing functionality. 
    /// </summary>
    public static class MathParser
    {
        // TODO: Add support for memory functions (preferably per user). 
        // TODO: Persist memory between bot runs. 
        public const string ValidChars = "0123456789+-*/";
        public const string OperChars = "+-*/";

        public static string Parse(string expr) {
            // First we validate the expression. 
            if (expr.IndexOfAny(ValidChars.ToArray()) == -1) {
                return "Invalid expression. ";
            }
            if (CharHelper.EqualsAny(expr[0], OperChars.ToArray()) ||
                CharHelper.EqualsAny(expr[expr.Length - 1], OperChars.ToArray())) {
                return "Invalid expression. ";
            }

            // Generate an Expression array and get the result out!
            return MathExpression.Result(expr).ToString();
        }
    }

    /// <summary>
    /// Math operators
    /// </summary>
    public enum MathOperators
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
    public class MathExpression
    {
        public const int MultDivide = 2;
        public const int AddSubtract = 1;
        public const int None = 0;

        private readonly List<MathTerm> _terms;

        public MathExpression(MathTerm[] terms) {
            _terms = terms.ToList();
        }

        public double Value {
            get {
                double retValue = 0;
                while (HasOpers) {
                    for (int i = 0; i < _terms.Count; i++) {
                        // If the working term isn't an oper don't continue. 
                        if (_terms[i].Operator == MathOperators.None) continue; 
                        // Using 3 terms at a time, get their value and remove them from the working list. 
                        // TODO: Remove the hardcoded precedence evaluation. 
                        // TODO: Add support for parentheses. 
                        // We evaluate the expression starting with the highest precedence of operators, and 
                        // evaluate those found, then work our way down to the lowest precendence. 
                        if (_terms[i].Operator == MathOperators.Multiply || _terms[i].Operator == MathOperators.Divide) {
                            if (_terms[i].Operator == MathOperators.Multiply) {
                                retValue = _terms[i - 1].Value * _terms[i + 1].Value;
                            } else if (_terms[i].Operator == MathOperators.Divide) {
                                retValue = _terms[i - 1].Value / _terms[i + 1].Value;
                            }
                            continueProc(retValue, i);
                        } else if ((_terms[i].Operator == MathOperators.Add || _terms[i].Operator == MathOperators.Subtract) &&
                                   HighestPrecedence <= AddSubtract) {
                            if (_terms[i].Operator == MathOperators.Add) {
                                retValue = _terms[i - 1].Value + _terms[i + 1].Value;
                            } else if (_terms[i].Operator == MathOperators.Subtract) {
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
                    foreach (MathTerm t in _terms) {
                        if (StringHelper.EqualsAny(t.TermString, new[] { "*", "/" })) {
                            retVal = MultDivide;
                        }
                        if (StringHelper.EqualsAny(t.TermString, new[] { "+", "-" }))
                        {
                            retVal = AddSubtract;
                        }
                    }
                }
                return retVal;
            }
        }

        public bool HasOpers {
            get { return _terms.Any(t => t.Operator != MathOperators.None); }
        }

        private void continueProc(double retValue, int i) {
            _terms.Insert(i + 2, new MathTerm(retValue.ToString()));
            _terms.RemoveRange(i - 1, 3);
        }

        public static double Result(string expr) {
            // Split expr by all oper characters and get the list of opers. 
            string[] termStrings = expr.Split(MathParser.OperChars.ToArray());
            List<MathTerm> numericTerms = termStrings.Select(s => new MathTerm(s)).ToList();
            List<MathTerm> operTerms = getOperators(expr);

            // Merge the two lists. 
            List<MathTerm> termsList = mergeTerms(numericTerms, operTerms);

            // Create the expression and get it's value. 
            var e = new MathExpression(termsList.ToArray());
            return e.Value;
        }

        private static List<MathTerm> mergeTerms(List<MathTerm> numericTerms, List<MathTerm> operTerms) {
            // If we have no operators, our Terms list is already a simple expression and 
            // can be simply returned. 
            if (operTerms.Count == 0) {
                return numericTerms;
            }

            var retValue = new List<MathTerm>();
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

        private static List<MathTerm> getOperators(string expr) {
            var operTerms = new List<MathTerm>();
            foreach (char c in expr) {
                if (CharHelper.EqualsAny(c, MathParser.OperChars.ToArray())) {
                    operTerms.Add(new MathTerm(c.ToString()));
                }
            }
            return operTerms;
        }
    }

    /// <summary>
    /// An expression term, such as '+', '33', or '*'. 
    /// </summary>
    public class MathTerm
    {
        public readonly string TermString;

        public MathTerm(string term) {
            TermString = term;
        }

        public MathOperators Operator {
            get {
                switch (TermString) {
                    case "+":
                        return MathOperators.Add;
                    case "-":
                        return MathOperators.Subtract;
                    case "*":
                        return MathOperators.Multiply;
                    case "/":
                        return MathOperators.Divide;
                    default:
                        return MathOperators.None;
                }
            }
        }

        /// <summary>
        /// Gets this Term's value. Do not call on an operatior term, or you'll get an 
        /// ArgumentException.
        /// </summary>
        public double Value {
            get {
                if (Operator == MathOperators.None) {
                    return Double.Parse(TermString);
                }
                throw new ArgumentException("This Term is an operator term.");
            }
        }
    }
}