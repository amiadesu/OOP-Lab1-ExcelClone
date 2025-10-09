using System.Collections.Generic;
using ExcelClone.Values;

namespace ExcelClone.Types;

public class ValueEvaluationResult
{
    public readonly CellValue Result;
    public readonly List<string> Dependencies;
    public readonly string? ErrorMessage;

    public ValueEvaluationResult(CellValue result, List<string> dependencies, string? errorMessage)
    {
        Result = result;
        Dependencies = dependencies;
        ErrorMessage = errorMessage;
    }
}