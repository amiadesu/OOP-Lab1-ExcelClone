using ExcelClone.Components;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Values;

namespace ExcelClone.Services
{
    public interface IFormulaParserService
    {
        (CellValue result, string? errorMessage) Evaluate(string formula);
    }
}