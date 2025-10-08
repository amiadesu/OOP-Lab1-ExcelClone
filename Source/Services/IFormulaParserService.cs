using System.Collections.Generic;
using ExcelClone.Components;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Values;

namespace ExcelClone.Services
{
    public interface IFormulaParserService
    {
        (CellValue result, List<string> dependencies, string? errorMessage) Evaluate(string formula);
    }
}