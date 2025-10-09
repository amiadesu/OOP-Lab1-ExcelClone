using System;
using ExcelClone.Resources.Localization;
using ExcelClone.Services;
using ExcelClone.Types;
using ExcelClone.Utils;
using ExcelClone.Values;

namespace ExcelClone.Components;

public class SpreadsheetCell {
    public CellValue Value { get; private set; } = new CellValue(CellValueType.Text);
    public string Formula { get; private set; } = "";

    public void SetFormula(string formula)
    {
        Formula = formula;
    }

    public ValueEvaluationResult UpdateValue(IFormulaParserService formulaParserService)
    {
        var result = formulaParserService.Evaluate(Formula);
        Value = result.Result;

        return result;
    }

    public void SetErrorValue(CellValueType errorType = CellValueType.GeneralError)
    {
        if (errorType != CellValueType.RefError && errorType != CellValueType.GeneralError)
        {
            throw new ArgumentException(
                DataProcessor.FormatResource(
                    AppResources.InvalidErrorTypeProvided,
                    ("ErrorType", errorType)
                )
            );
        }
        Value = new CellValue(errorType);
    }

    public void Clear()
    {
        Value.Clear();
        Formula = "";
    }
}