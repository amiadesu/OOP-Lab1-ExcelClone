using System;
using System.Collections.Generic;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Constants;
using ExcelClone.Values;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Exceptions;

namespace ExcelClone.Evaluators;

public interface IFormulaEvaluator
{
    public CellValue Evaluate(List<Token> tokens);
}
