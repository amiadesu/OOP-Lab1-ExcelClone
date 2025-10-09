using System;
using System.Collections.Generic;
using ExcelClone.Resources.Localization;
using ExcelClone.Services;
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

    public (CellValue result, List<string> dependencies, string? errorMessage) UpdateValue(IFormulaParserService formulaParserService)
    {
        var result = formulaParserService.Evaluate(Formula);
        Value = result.result;

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