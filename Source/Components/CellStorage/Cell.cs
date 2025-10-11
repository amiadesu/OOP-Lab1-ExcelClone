using System;
using System.Diagnostics;
using ExcelClone.Resources.Localization;
using ExcelClone.Services;
using ExcelClone.Types;
using ExcelClone.Utils;
using ExcelClone.Values;

namespace ExcelClone.Components.CellStorage;

public class SpreadsheetCell
{
    public CellValue Value { get; private set; } = new CellValue(CellValueType.Text);
    public string Formula { get; private set; } = "";

    public void SetFormula(string formula)
    {
        Formula = formula;
    }

    public ValueEvaluationResult UpdateValue(IFormulaParserService formulaParserService)
    {
        var result = formulaParserService.Evaluate(Formula);

        Trace.TraceInformation($"Before assigment: {Value.GetDescription()}");
        Value = result.Result;
        Trace.TraceInformation($"After assigment: {Value.GetDescription()}");

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

    public void CopyFrom(SpreadsheetCell other)
    {
        Value.CopyFrom(other.Value);
        Formula = other.Formula;
    }

    public void Clear()
    {
        Value.Clear();
        Formula = "";
    }
}