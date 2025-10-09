using System.Collections.Generic;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Values;

namespace ExcelClone.Evaluators;

public interface IFormulaEvaluator
{
    public CellValue Evaluate(List<Token> tokens);
}
