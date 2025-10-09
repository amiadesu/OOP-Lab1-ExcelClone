using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Constants;
using ExcelClone.Values;
using System.Linq;
using ExcelClone.Types;

namespace ExcelClone.Services;

public class FormulaParserService : IFormulaParserService
{
    private readonly IFormulaTokenizer _formulaTokenizer;
    private readonly IFormulaEvaluator _formulaEvaluator;

    public FormulaParserService(IFormulaTokenizer tokenizer, IFormulaEvaluator evaluator)
    {
        _formulaTokenizer = tokenizer;
        _formulaEvaluator = evaluator;
    }

    public ValueEvaluationResult Evaluate(string formula)
    {
        List<string> dependencies = new();
        if (formula.StartsWith(Literals.prefix))
        {
            try
            {
                var expression = formula[Literals.prefixLength..];
                var tokens = _formulaTokenizer.Tokenize(expression);

                dependencies = GetCellDependencies(tokens);

                var result = EvaluateExpression(tokens);
                return new ValueEvaluationResult(result.result, dependencies, result.errorMessage);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Formula evaluation error: {e.Message}");
                return new ValueEvaluationResult(new CellValue(Literals.errorMessage), dependencies, e.Message);
            }
        }

        return new ValueEvaluationResult(new CellValue(formula), dependencies, null);
    }

    private static List<string> GetCellDependencies(List<Token> tokens)
    {
        List<string> dependencies = new();

        foreach (var token in tokens.Where(t => t.Type == TokenType.CellReference))
        {
            dependencies.Add(token.Value);
        }

        return dependencies;
    }

    private (CellValue result, string? errorMessage) EvaluateExpression(List<Token> tokens)
    {
        try
        {
            CellValue result = _formulaEvaluator.Evaluate(tokens);
            return (result, null);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            return (new CellValue(Literals.errorMessage), e.Message);
        }
    }
}