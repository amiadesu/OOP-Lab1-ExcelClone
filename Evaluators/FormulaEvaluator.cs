using System;
using System.Collections.Generic;
using System.Linq;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Utils;
using ExcelClone.Constants;

namespace ExcelClone.Evaluators
{
    public static class FormulaEvaluator
    {
        public static double Evaluate(List<Token> tokens)
        {
            var parser = new Parser(tokens);
            double result = parser.ParseExpression();

            if (!parser.IsAtEnd())
                throw new Exception($"Unexpected token '{parser.Current?.Value}' at the end of expression");

            return result;
        }

        private class Parser
        {
            private readonly List<Token> _tokens;
            private int _pos;

            public Parser(List<Token> tokens)
            {
                _tokens = tokens;
                _pos = 0;
            }

            public bool IsAtEnd() => _pos >= _tokens.Count;
            public Token? Current => !IsAtEnd() ? _tokens[_pos] : null;
            private void Next() => _pos++;

            private bool Match(TokenType type, string? value = null)
            {
                if (IsAtEnd()) return false;
                if (Current!.Type == type && (value == null || Current.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
                {
                    Next();
                    return true;
                }
                return false;
            }

            private Token Consume(TokenType type, string? value = null)
            {
                if (!Match(type, value))
                    throw new Exception($"Expected {type}{(value != null ? $" '{value}'" : "")}, found '{Current?.Value}'");
                return _tokens[_pos - 1];
            }

            // -------------------
            // Entry point
            // -------------------
            public double ParseExpression() => ParseBinaryExpression(0);

            // -------------------
            // Binary expression parser (handles operators & infix functions)
            // -------------------
            private double ParseBinaryExpression(int minPrecedence)
            {
                double left = ParseUnary();

                while (!IsAtEnd())
                {
                    if (!TryGetOperator(out string op, out int prec, out Associativity assoc) || prec < minPrecedence)
                        break;

                    Next(); // consume operator/function

                    // Choose nextMinPrec depending on associativity
                    int nextMinPrec = (assoc == Associativity.Left) ? prec + 1 : prec;

                    double right = ParseBinaryExpression(nextMinPrec);

                    left = EvaluateOperatorOrFunction(op, left, right);
                }

                return left;
            }

            private bool TryGetOperator(out string op, out int precedence, out Associativity assoc)
            {
                op = Current!.Value.ToUpperInvariant();

                // Two-character comparators (<=, >=, <>)
                if (IsExtendableComparisonOperator(op) && _pos + 1 < _tokens.Count)
                {
                    string next = _tokens[_pos + 1].Value;
                    if ((op == "<" && (next == "=" || next == ">")) || (op == ">" && next == "="))
                    {
                        op += next;
                        Next(); // consume second token
                    }
                }

                if (Functions.infixFunctionsInfo.TryGetValue(op, out var info))
                {
                    precedence = info.precedence;
                    assoc = info.associativity;
                    return true;
                }

                precedence = 0;
                assoc = Associativity.Left;
                return false;
            }

            private bool IsExtendableComparisonOperator(string op)
            {
                return Current?.Type == TokenType.Operator && (op == "<" || op == ">");
            }

            private static double EvaluateOperatorOrFunction(string op, double left, double right)
            {
                if (Functions.operatorFunctions.TryGetValue(op, out var func))
                    return func(left, right);

                throw new Exception($"Unknown operator or infix function '{op}'");
            }

            // -------------------
            // Unary +/-
            // -------------------
            private double ParseUnary()
            {
                if (!IsAtEnd() && Current!.Type == TokenType.Operator)
                {
                    if (Current.Value == "+") { Next(); return ParseUnary(); }
                    if (Current.Value == "-") { Next(); return -ParseUnary(); }
                }

                return ParsePrimary();
            }

            // -------------------
            // Primary: number, parentheses, prefix function
            // -------------------
            private double ParsePrimary()
            {
                if (IsAtEnd()) throw new Exception("Unexpected end of expression");
                Token token = Current!;

                // Number
                if (token.Type == TokenType.Number)
                {
                    Next();
                    return double.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture);
                }

                // Parentheses
                if (token.Type == TokenType.Parenthesis && token.Value == "(")
                {
                    Next();
                    double value = ParseExpression();
                    Consume(TokenType.Parenthesis, ")");
                    return value;
                }

                // Prefix function
                if (token.Type == TokenType.Function && Functions.prefixNotationFunctions.Contains(token.Value.ToUpperInvariant()))
                {
                    string name = token.Value.ToUpperInvariant();
                    Next();
                    return ParsePrefixFunction(name);
                }

                throw new Exception($"Unexpected token '{token.Value}'");
            }

            // -------------------
            // Prefix function parser: e.g., SUM(1,2)
            // -------------------
            private double ParsePrefixFunction(string name)
            {
                Consume(TokenType.Parenthesis, "(");
                var args = new List<double>();

                if (!Match(TokenType.Parenthesis, ")"))
                {
                    do
                    {
                        args.Add(ParseExpression());
                    } while (Match(TokenType.Comma));

                    Consume(TokenType.Parenthesis, ")");
                }

                return EvaluatePrefixFunction(name, args.ToArray());
            }

            private static double EvaluatePrefixFunction(string name, double[] args)
            {
                if (Functions.prefixFunctions.TryGetValue(name, out var func))
                    return func(args);

                throw new Exception($"Unknown prefix function '{name}'");
            }
        }
    }
}
