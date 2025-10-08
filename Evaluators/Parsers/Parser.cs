using System;
using System.Collections.Generic;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Constants;
using ExcelClone.Values;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Exceptions;
using ExcelClone.Components;

namespace ExcelClone.Evaluators.Parsers;

public class Parser : IParser
{
    private readonly ICellStorageReader? _reader;
    private List<Token>? _tokens;
    private int _pos = 0;

    public Parser(ICellStorageReader? reader = null)
    {
        _reader = reader;
    }

    private void Initialize(List<Token> tokens)
    {
        _tokens = tokens;
        _pos = 0;
    }

    public bool IsAtEnd() => _pos >= _tokens?.Count;
    public Token? CurrentToken() => !IsAtEnd() ? _tokens?[_pos] : null;
    private void Next() => _pos++;

    private bool Match(TokenType type, string? value = null)
    {
        if (IsAtEnd()) return false;
        if (CurrentToken()!.Type == type &&
            (value == null || CurrentToken()!.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            Next();
            return true;
        }
        return false;
    }

    private void Consume(TokenType type, string? value = null)
    {
        if (!Match(type, value))
            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.UnexpectedValue,
                ("Type", type),
                ("ExpectedValue", value != null ? $" '{value}'" : ""),
                ("RealValue", CurrentToken()?.Value ?? "")
            ));
    }

    // Entry point
    public CellValue ParseExpression(List<Token> tokens)
    {
        Initialize(tokens);
        return ParseExpressionInner();
    }

    private CellValue ParseExpressionInner() => ParseBinaryExpression(0);

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
        op = CurrentToken()!.Value.ToUpperInvariant();

        // Two-character comparators (<=, >=, <>)
        if (IsExtendableComparisonOperator(op) && _pos + 1 < _tokens?.Count)
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
        return CurrentToken()?.Type == TokenType.Operator && (op == "<" || op == ">");
    }

    // Unary +/-
    private CellValue ParseUnary()
    {
        if (!IsAtEnd() && CurrentToken()!.Type == TokenType.Operator)
        {
            if (CurrentToken()!.Value == "+") { Next(); return ParseUnary(); }
            if (CurrentToken()!.Value == "-") { Next(); return -ParseUnary(); }
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
        Token token = CurrentToken()!;

        // Number
        if (token.Type == TokenType.Number)
        {
            Next();
            return new CellValue(
                CellValueType.Number,
                numberValue: DataProcessor.StringToDouble(token.Value)
            );
        }

        if (token.Type == TokenType.CellReference)
        {
            if (_reader is null)
            {
                return new CellValue(Literals.refErrorMessage);
            }

            CellValue? realCellValue = _reader.GetCellValue(token.Value);

            if (realCellValue is null)
            {
                return new CellValue(Literals.refErrorMessage);
            }

            if (StringChecker.IsError(realCellValue.Value))
            {
                return new CellValue(Literals.errorMessage);
            }

            return realCellValue;
        }

        // Parentheses
        if (token.Type == TokenType.Parenthesis && token.Value == "(")
        {
            Next();
            CellValue value = ParseExpressionInner();
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
                args.Add(ParseExpressionInner());
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