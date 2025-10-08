using System.Collections.Generic;
using System.Linq;
using ExcelClone.Services;
using ExcelClone.Constants;
using ExcelClone.Values;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Utils;
using Microsoft.Maui.Controls;

namespace ExcelClone.Components;

public class Spreadsheet : ICellStorage
{
    private readonly Dictionary<string, (SpreadsheetCell cell, string name)> _cells;
    private readonly ICellNameService _cellNameService;
    public int Columns { get; private set; } = 0;
    public int Rows { get; private set; } = 0;

    public Spreadsheet(ICellNameService cellNameService)
    {
        this._cellNameService = cellNameService;
        _cells = new Dictionary<string, (SpreadsheetCell cell, string name)>();
    }

    public void CreateNewCellStorage(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
        Clear();
        InitializeCells();
    }

    public int GetColumns()
    {
        return Columns;
    }
    public int GetRows()
    {
        return Rows;
    }

    public string GetCellName(int col, int row) {
        return _cellNameService.GetCellName(col, row);
    }

    private void InitializeCells()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                string cellName = _cellNameService.GetCellName(col, row);
                _cells[cellName] = (new SpreadsheetCell(), cellName);
            }
        }
    }

    private void Clear()
    {
        _cells.Clear();
    }

    public (SpreadsheetCell cell, string name)? GetCell(string cellName)
    {
        return CellExists(cellName) ? _cells[cellName.ToUpper()] : null;
    }

    public void SetCellFormula(string cellReference, string formula) {
        if (!CellExists(cellReference))
            return;

        var cellObject = _cells[cellReference.ToUpper()];
        
        cellObject.cell.SetFormula(formula);
    }
    public void SetCellValue(string cellReference, CellValue value)
    {
        if (!CellExists(cellReference))
            return;

        var cellObject = _cells[cellReference.ToUpper()];
        
        cellObject.cell.SetValue(value);
    }

    public string GetCellDisplayValue(string cellReference)
    {
        var cellObject = GetCell(cellReference);
        return cellObject?.cell.Value.ToString() ?? Literals.refErrorMessage;
    }

    public CellValue? GetCellValue(string cellReference)
    {
        var cellObject = GetCell(cellReference);
        return cellObject?.cell.Value ?? null;
    }

    public string GetCellFormula(string cellReference)
    {
        var cellObject = GetCell(cellReference);
        return cellObject?.cell.Formula ?? "";
    }

    private bool CellExists(string cellName)
    {
        return _cells.ContainsKey(cellName.ToUpper());
    }
}