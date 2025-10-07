using System;
using System.Collections.Generic;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Constants;
using ExcelClone.Values;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Exceptions;

namespace ExcelClone.Evaluators;

public static class FormulaEvaluator
{
    public static CellValue Evaluate(List<Token> tokens)
    {
        var parser = new Parser(tokens);
        CellValue result = parser.ParseExpression();

        if (!parser.IsAtEnd())
            throw new FormulaParseException(DataProcessor.FormatResource(
                AppResources.UnexpectedTokenEOE,
                ("Token", parser.Current?.Value ?? "")
            ));

        return result;
    }

    private sealed class Parser
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
                throw new ArgumentException(DataProcessor.FormatResource(
                    AppResources.UnexpectedValue,
                    ("Type", type),
                    ("ExpectedValue", value != null ? $" '{value}'" : ""),
                    ("RealValue", Current?.Value ?? "")
                ));
                
            return _tokens[_pos - 1];
        }

        // Entry point
        public CellValue ParseExpression() => ParseBinaryExpression(0);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Operator-precedence_parser
        /// </summary>
        private CellValue ParseBinaryExpression(int minPrecedence)
        {
            CellValue left = ParseUnary();

            while (!IsAtEnd())
            {
                if (!TryGetOperator(out string op, out int prec, out Associativity assoc) || prec < minPrecedence)
                    break;

                Next(); // consume operator/function

                // Choose nextMinPrec depending on associativity
                int nextMinPrec = (assoc == Associativity.Left) ? prec + 1 : prec;

                CellValue right = ParseBinaryExpression(nextMinPrec);

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

        // Unary +/-
        private CellValue ParseUnary()
        {
            if (!IsAtEnd() && Current!.Type == TokenType.Operator)
            {
                if (Current.Value == "+") { Next(); return ParseUnary(); }
                if (Current.Value == "-") { Next(); return -ParseUnary(); }
            }

            return ParsePrimary();
        }

        // Primary: number, parentheses, prefix function
        private CellValue ParsePrimary()
        {
            if (IsAtEnd())
            {
                throw new FormulaParseException(DataProcessor.FormatResource(
                    AppResources.UnexpectedEOE
                ));
            }
            Token token = Current!;

            // Number
            if (token.Type == TokenType.Number)
            {
                Next();
                return new CellValue(
                    CellValueType.Number,
                    numberValue: DataProcessor.StringToDouble(token.Value)
                );
            }

            if (token.Type == TokenType.CellValue)
            {
                Next();
                return token.CellValue!;
            }

            // Parentheses
            if (token.Type == TokenType.Parenthesis && token.Value == "(")
            {
                Next();
                CellValue value = ParseExpression();
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

            throw new FormulaParseException(DataProcessor.FormatResource(
                AppResources.UnexpectedToken,
                ("Token", token.Value)
            ));
        }

        // Prefix function parser: e.g., SUM(1,2)
        private CellValue ParsePrefixFunction(string name)
        {
            Consume(TokenType.Parenthesis, "(");
            var args = new List<CellValue>();

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

        private static CellValue EvaluateOperatorOrFunction(string op, CellValue left, CellValue right)
        {
            if (Functions.operatorFunctions.TryGetValue(op, out var func))
                return func(left, right);

            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.UnknownInfixFunction,
                ("FunctionName", op)
            ));
        }

        private static CellValue EvaluatePrefixFunction(string name, CellValue[] args)
        {
            if (Functions.prefixFunctions.TryGetValue(name, out var func))
                return func(args);

            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.UnknownPrefixFunction,
                ("FunctionName", name)
            ));
        }
    }
}
