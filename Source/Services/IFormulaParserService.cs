using ExcelClone.Types;

namespace ExcelClone.Services;

public interface IFormulaParserService
{
    ValueEvaluationResult Evaluate(string formula);
}