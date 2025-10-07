using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExcelClone.Components;
using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Automatons;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Utils;
using ExcelClone.Constants;
using ExcelClone.Values;

namespace ExcelClone.Services;

public class FormulaParserService : IFormulaParserService
{
    private readonly Spreadsheet _currentSpreadsheet;
    private readonly IFormulaTokenizer _formulaTokenizer;

    public FormulaParserService(Spreadsheet spreadsheet, IFormulaTokenizer tokenizer)
    {
        _currentSpreadsheet = spreadsheet;
        _formulaTokenizer = tokenizer;
    }

    public (CellValue result, string? errorMessage) Evaluate(string formula)
    {
        if (formula.StartsWith(Literals.prefix))
        {
            try
            {
                var expression = formula[Literals.prefixLength..];
                var tokens = _formulaTokenizer.Tokenize(expression);
                var result = EvaluateExpression(tokens);
                return (new CellValue(result.result), result.errorMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Formula evaluation error: {e.Message}");
                return (new CellValue(Literals.errorMessage), e.Message);
            }
        }

        return (new CellValue(formula), null);
    }

    private (string result, string? errorMessage) EvaluateExpression(List<Token> tokens)
    {
        int idx = 0;
        while (idx < tokens.Count)
        {
            Token token = tokens[idx];

            if (token.Type == TokenType.CellReference)
            {
                CellValue? realCellValue = _currentSpreadsheet.GetCellRealValue(token.Value);

                if (realCellValue is null)
                {
                    return (Literals.refErrorMessage, null);
                }

                if (StringChecker.IsError(realCellValue.Value))
                {
                    return (Literals.errorMessage, null);
                }

                tokens[idx] = new Token
                {
                    Type = TokenType.CellValue,
                    CellValue = realCellValue,
                    Position = token.Position
                };
            }

            idx++;
        }

        try
        {
            CellValue result = FormulaEvaluator.Evaluate(tokens);
            return (result, null);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return (Literals.errorMessage, e.Message);
        }
    }
}