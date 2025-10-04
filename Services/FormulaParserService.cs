using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExcelClone.Components;
using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Automatons;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Utils;
using ExcelClone.Constants;

namespace ExcelClone.Services;

public class FormulaParserService : IFormulaParserService
{
    private Spreadsheet _currentSpreadsheet;
    private string _currentCell;

    private readonly FormulaTokenizer _formulaTokenizer = new FormulaTokenizer();

    private readonly NumberAutomaton _numberAutomaton = new NumberAutomaton();

    public string Evaluate(string formula, string currentCell, Spreadsheet spreadsheet)
    {
        if (formula.StartsWith(Literals.prefix))
        {
            try
            {
                _currentSpreadsheet = spreadsheet;
                _currentCell = currentCell;

                var expression = formula[Literals.prefixLength..];
                var tokens = _formulaTokenizer.Tokenize(expression);
                return EvaluateExpression(tokens);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Formula evaluation error: {e.Message}");
                return "#ERROR!";
            }
        }

        return formula;
    }

    private string EvaluateExpression(List<Token> tokens)
    {
        int idx = 0;
        while (idx < tokens.Count)
        {
            Token token = tokens[idx];

            switch (token.Type)
            {
                case TokenType.CellReference:
                    string realCellValue = _currentSpreadsheet.GetCellDisplayValue(token.Value);
                    string cellValue = realCellValue;
                    if (cellValue.StartsWith('-') || cellValue.StartsWith('+'))
                    {
                        cellValue = cellValue[1..];
                    }
                    if (_numberAutomaton.TestString(cellValue))
                    {
                        tokens[idx] = new Token
                        {
                            Type = TokenType.Number,
                            Value = realCellValue,
                            Position = token.Position
                        };
                    }
                    else if (StringChecker.IsError(realCellValue))
                    {
                        return Literals.errorMessage;
                    }
                    else
                    {
                        tokens[idx] = new Token
                        {
                            Type = TokenType.Text,
                            Value = realCellValue,
                            Position = token.Position
                        };
                    }
                    break;
            }

            idx++;
        }

        try
        {
            double result = FormulaEvaluator.Evaluate(tokens);
            return DataFormatter.FormatFloatingPoint(result);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return Literals.errorMessage;
        }
    }
}