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
    private Spreadsheet _currentSpreadsheet;
    private string _currentCell;

    private readonly FormulaTokenizer _formulaTokenizer = new FormulaTokenizer();

    private readonly NumberAutomaton _numberAutomaton = new NumberAutomaton();

    public CellValue Evaluate(string formula, string currentCell, Spreadsheet spreadsheet, ref string? errorMessage)
    {
        if (formula.StartsWith(Literals.prefix))
        {
            try
            {
                _currentSpreadsheet = spreadsheet;
                _currentCell = currentCell;

                var expression = formula[Literals.prefixLength..];
                var tokens = _formulaTokenizer.Tokenize(expression);
                return EvaluateExpression(tokens, ref errorMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Formula evaluation error: {e.Message}");
                return Literals.errorMessage;
            }
        }

        return formula;
    }

    private string EvaluateExpression(List<Token> tokens, ref string? errorMessage)
    {
        int idx = 0;
        while (idx < tokens.Count)
        {
            Token token = tokens[idx];

            switch (token.Type)
            {
                case TokenType.CellReference:
                    CellValue? realCellValue = _currentSpreadsheet.GetCellRealValue(token.Value);
                    if (realCellValue is null)
                    {
                        return Literals.refErrorMessage;
                    }
                    if (StringChecker.IsSignedNumber(realCellValue.Value))
                    {
                        tokens[idx] = new Token
                        {
                            Type = TokenType.CellValue,
                            CellValue = realCellValue,
                            Position = token.Position
                        };
                    }
                    else if (StringChecker.IsError(realCellValue.Value))
                    {
                        return Literals.errorMessage;
                    }
                    else
                    {
                        tokens[idx] = new Token
                        {
                            Type = TokenType.CellValue,
                            CellValue = realCellValue,
                            Position = token.Position
                        };
                    }
                    break;
            }

            idx++;
        }

        try
        {
            CellValue result = FormulaEvaluator.Evaluate(tokens);
            return result;
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            errorMessage = e.Message;
            return Literals.errorMessage;
        }
    }
}