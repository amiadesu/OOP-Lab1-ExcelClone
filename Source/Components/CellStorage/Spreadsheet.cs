using System.Collections.Generic;
using ExcelClone.Services;
using ExcelClone.Constants;
using ExcelClone.Values;
using ExcelClone.Types;
using System;

namespace ExcelClone.Components.CellStorage;

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
        var oldCells = new Dictionary<string, (SpreadsheetCell cell, string name)>(_cells);
        int oldCols = Columns;
        int oldRows = Rows;

        Columns = columns;
        Rows = rows;
        Clear();
        InitializeCells();

        RestoreCellsFrom(oldCols, oldRows, oldCells);
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

    private void RestoreCellsFrom(int columns, int rows, Dictionary<string, (SpreadsheetCell cell, string name)> cellsSource)
    {
        for (int row = 0; row < Math.Min(Rows, rows); row++)
        {
            for (int col = 0; col < Math.Min(Columns, columns); col++)
            {
                string cellName = _cellNameService.GetCellName(col, row);
                string upperName = cellName.ToUpper();

                if (!cellsSource.TryGetValue(upperName, out var oldCell))
                {
                    continue;
                }
                
                _cells[upperName].cell.CopyFrom(oldCell.cell);
            }
        }
    }

    public (SpreadsheetCell cell, string name)? GetCell(string cellName)
    {
        return CellExists(cellName) ? _cells[cellName.ToUpper()] : null;
    }

    public void SetCellFormula(string cellReference, string formula)
    {
        if (!CellExists(cellReference))
            return;

        var cellObject = _cells[cellReference.ToUpper()];

        cellObject.cell.SetFormula(formula);
    }
    public void SetCellErrorValue(string cellReference, CellValueType errorType = CellValueType.GeneralError)
    {
        if (!CellExists(cellReference))
            return;

        var cellObject = _cells[cellReference.ToUpper()];

        cellObject.cell.SetErrorValue(errorType);
    }
    public ValueEvaluationResult? UpdateCellValue(string cellReference, IFormulaParserService formulaParserService)
    {
        if (!CellExists(cellReference))
            return null;

        var cellObject = _cells[cellReference.ToUpper()];
        
        return cellObject.cell.UpdateValue(formulaParserService);
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