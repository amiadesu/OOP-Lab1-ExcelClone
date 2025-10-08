using ExcelClone.Values;

namespace ExcelClone.Components;

public interface ICellStorageWriter
{
    public void SetCellFormula(string cellReference, string formula);
    public void SetCellValue(string cellReference, CellValue value);
}