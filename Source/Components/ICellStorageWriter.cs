using System.Collections.Generic;
using ExcelClone.Services;
using ExcelClone.Values;

namespace ExcelClone.Components;

public interface ICellStorageWriter
{
    public void SetCellFormula(string cellReference, string formula);
    public void SetCellErrorValue(string cellReference, CellValueType errorType = CellValueType.GeneralError);
    public (CellValue result, List<string> dependencies, string? errorMessage)? UpdateCellValue(string cellReference, IFormulaParserService formulaParserService);
}