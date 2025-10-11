using ExcelClone.Services;
using ExcelClone.Types;
using ExcelClone.Values;

namespace ExcelClone.Components.CellStorage;

public interface ICellStorageWriter
{
    public void SetCellFormula(string cellReference, string formula);
    public void SetCellErrorValue(string cellReference, CellValueType errorType = CellValueType.GeneralError);
    public ValueEvaluationResult? UpdateCellValue(string cellReference, IFormulaParserService formulaParserService);
}