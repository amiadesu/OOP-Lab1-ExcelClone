using System.Collections.Generic;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Values;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Exceptions;
using ExcelClone.Evaluators.Parsers;

namespace ExcelClone.Evaluators;

public class FormulaEvaluator : IFormulaEvaluator
{
    private readonly IParser _parser;

    public FormulaEvaluator(IParser parser)
    {
        _parser = parser;
    }

    public CellValue Evaluate(List<Token> tokens)
    {
        CellValue result = _parser.ParseExpression(tokens);

        if (!_parser.IsAtEnd())
            throw new FormulaParseException(DataProcessor.FormatResource(
                AppResources.UnexpectedTokenEOE,
                ("Token", _parser.CurrentToken()?.Value ?? "")
            ));

        return result;
    }
}
