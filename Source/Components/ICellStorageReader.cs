using ExcelClone.Values;

namespace ExcelClone.Components;

public interface ICellStorageReader
{
    public int GetColumns();
    public int GetRows();
    public string GetCellName(int col, int row);
    public CellValue? GetCellValue(string cellReference);
    public string GetCellDisplayValue(string cellReference);
    public string GetCellFormula(string cellReference);
}