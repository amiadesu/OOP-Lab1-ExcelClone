using System.Collections.Generic;

namespace ExcelClone.Services;

public interface ICellNameService
{
    // Cell naming
    string GetCellName(int columnIndex, int rowIndex);
    (int columnIndex, int rowIndex) ParseCellName(string cellName);
    bool IsValidCellName(string cellName);

    // Column naming
    string GetColumnName(int columnIndex);
    int GetColumnIndex(string columnName);

    // Row naming
    string GetRowName(int rowIndex);
    int GetRowIndex(string rowName);

    // Range operations
    string GetCellRange(string startCell, string endCell);
    string[] ParseCellRange(string range);

    // Generators
    IEnumerable<string> CellNames(int cols, int rows);
}