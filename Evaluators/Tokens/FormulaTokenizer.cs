using System;
using System.Collections.Generic;
using System.Linq;
using ExcelClone.Evaluators.Automatons;

using System.Diagnostics;
using ExcelClone.Constants;

namespace ExcelClone.Evaluators.Tokens;

public class FormulaTokenizer
{
    private enum TokenParserState
    {
        Start,
        InNumber,
        InText,
        InCellReference,
        InFunction,
        InOperator,
        InParenthesis,
        Complete,
        Error
    }

    private readonly NumberAutomaton _numberAutomaton = new NumberAutomaton();
    private readonly CellNameAutomaton _cellNameAutomaton = new CellNameAutomaton();
    private readonly FunctionNameAutomaton _functionNameAutomaton = new FunctionNameAutomaton();

    public List<Token> Tokenize(string expression)
    {
        _numberAutomaton.Reset();
        _cellNameAutomaton.Reset();
        _functionNameAutomaton.Reset();

        var tokens = new List<Token>();
        int position = 0;

        TokenParserState state = TokenParserState.Start;

        // Logic
        while (position < expression.Length)
        {
            char c = expression[position];

            Trace.WriteLine($"{position}, {c}");

            switch (state)
            {
                case TokenParserState.Start:
                    ProcessChar(c, ref position, ref tokens, ref state);
                    break;

                case TokenParserState.InNumber:
                    tokens.Add(ReadNumber(expression, ref position));

                    state = TokenParserState.Start;
                    break;

                case TokenParserState.InText:
                    tokens.Add(ReadText(expression, ref position));

                    state = TokenParserState.Start;
                    break;

                case TokenParserState.InCellReference:
                    tokens.Add(ReadCellReference(expression, ref position));

                    state = TokenParserState.Start;
                    break;

                case TokenParserState.InFunction:
                    tokens.Add(ReadFunction(expression, ref position));

                    state = TokenParserState.Start;
                    break;

                case TokenParserState.InOperator:
                    break;

                case TokenParserState.InParenthesis:
                    break;
            }
        }

        foreach (var token in tokens) {
            Trace.WriteLine($"{token.Type}, {token.Value}");
        }

        return tokens;
    }

    private void ProcessChar(char c, ref int position, ref List<Token> tokens, ref TokenParserState state)
    {
        if (char.IsWhiteSpace(c))
        {
            position++;
        }
        else if (_numberAutomaton.TestChar(c))
        {
            state = TokenParserState.InNumber;
        }
        else if (_cellNameAutomaton.TestChar(c))
        {
            state = TokenParserState.InCellReference;
        }
        else if (_functionNameAutomaton.TestChar(c))
        {
            state = TokenParserState.InFunction;
        }
        else if (c == '"')
        {
            state = TokenParserState.InText;
            position++; // Skip opening quote
        }
        else if (IsOperator(c))
        {
            tokens.Add(new Token { Type = TokenType.Operator, Value = c.ToString(), Position = position });
            position++;
        }
        else if (c == '(' || c == ')')
        {
            tokens.Add(new Token { Type = TokenType.Parenthesis, Value = c.ToString(), Position = position });
            position++;
        }
        else if (c == ',')
        {
            tokens.Add(new Token { Type = TokenType.Comma, Value = ",", Position = position });
            position++;
        }
        else if (c == ':')
        {
            tokens.Add(new Token { Type = TokenType.Colon, Value = ":", Position = position });
            position++;
        }
        else
        {
            throw new FormatException($"Unexpected character '{c}' at position {position}");
        }
    }

    private Token ReadNumber(string expression, ref int position)
    {
        string number = ReadWhile(expression, ref position, c => _numberAutomaton.Insert(c) != AutomatonState.Rejecting);

        Token token = new Token
        {
            Type = TokenType.Number,
            Value = number,
            Position = position
        };

        _numberAutomaton.Reset();

        return token;
    }

    private Token ReadText(string expression, ref int position)
    {
        string text = ReadWhile(expression, ref position, c => c != '"');
        Token token = new Token
        {
            Type = TokenType.Text,
            Value = text,
            Position = position
        };

        position++; // Skip closing quote

        return token;
    }

    private Token ReadCellReference(string expression, ref int position)
    {
        string reference = ReadWhile(expression, ref position, c => _cellNameAutomaton.Insert(c) != AutomatonState.Rejecting);
        Token token;

        // Check if it's a function (automaton is still processing or parenthesis follows reference) 
        // or cell reference
        if (!_cellNameAutomaton.TestString(reference) ||
            (position < expression.Length && expression[position] == '('))
        {
            token = new Token { Type = TokenType.Function, Value = reference.ToUpper(), Position = position };
        }
        else
        {
            token = new Token { Type = TokenType.CellReference, Value = reference.ToUpper(), Position = position };
        }

        _cellNameAutomaton.Reset();

        return token;
    }

    private Token ReadFunction(string expression, ref int position)
    {
        string function = ReadWhile(expression, ref position, c => _functionNameAutomaton.Insert(c) != AutomatonState.Rejecting);

        Token token = new Token { Type = TokenType.Function, Value = function.ToUpper(), Position = position };

        _functionNameAutomaton.Reset();

        return token;
    }

    private static string ReadWhile(string input, ref int position, Func<char, bool> condition)
    {
        int start = position;
        while (position < input.Length && condition(input[position]))
        {
            position++;
        }
        return input.Substring(start, position - start);
    }

    private static bool IsOperator(char c)
    {
        return Functions.basicOperators.Any(op => op == c);
    }
}