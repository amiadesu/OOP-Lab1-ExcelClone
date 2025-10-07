using System.Collections.Generic;
using System.Linq;
using ExcelClone.Services;
using ExcelClone.Constants;
using ExcelClone.Values;
using ExcelClone.Evaluators.Tokens;

namespace ExcelClone.Components;

public class Spreadsheet
{
    private readonly Dictionary<string, (ExcelCell cell, CellValue value, string name)> _cells;
    private readonly IFormulaParserService _formulaParser;
    public readonly ICellNameService cellNameService;
    public int Columns { get; private set; }
    public int Rows { get; private set; }

    public Spreadsheet(int columns, int rows, IFormulaTokenizer tokenizer, ICellNameService cellNameService)
    {
        this.cellNameService = cellNameService;
        Columns = columns;
        Rows = rows;
        _formulaParser = new FormulaParserService(this, tokenizer);
        _cells = new Dictionary<string, (ExcelCell cell, CellValue value, string name)>();
        InitializeCells();
    }

    private void InitializeCells()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                string cellName = cellNameService.GetCellName(col, row);
                _cells[cellName] = (new ExcelCell(), new CellValue(CellValueType.Text), cellName);
            }
        }
    }

    public (ExcelCell cell, CellValue value, string name)? GetCell(string cellName)
    {
        return _cells.ContainsKey(cellName.ToUpper()) ? _cells[cellName.ToUpper()] : null;
    }

    public string? SetCellValue(string cellName, string value)
    {
        if (!_cells.ContainsKey(cellName.ToUpper()))
            return null;

        var cellObject = _cells[cellName.ToUpper()];

        string? errorMessage = null;

        if (value.StartsWith(Literals.prefix))
        {
            cellObject.cell.Formula = value;

            var result = _formulaParser.Evaluate(value);
            cellObject.value.Value = result.result;
            
            errorMessage = result.errorMessage;
        }
        else
        {
            cellObject.cell.Formula = value;
            cellObject.value.Value = value;
        }

        // Recalculate cells
        string? possibleError = RecalculateCells();
        if (possibleError is not null && errorMessage is null)
        {
            errorMessage = possibleError;
        }

        return errorMessage;
    }

    public string GetCellDisplayValue(string cellName)
    {
        var cellObject = GetCell(cellName);
        return cellObject?.value.Value.ToString() ?? Literals.refErrorMessage;
    }

    public string GetCellFormula(string cellName)
    {
        var cellObject = GetCell(cellName);
        return cellObject?.cell.Formula ?? "";
    }

    public CellValue? GetCellRealValue(string cellName)
    {
        var cellObject = GetCell(cellName);
        return cellObject?.value ?? null;
    }

    private string? RecalculateCells()
    {
        string? errorMessage = null;

        // Recalculate all formula cells
        foreach (var cellObject in _cells.Values.Where(c => !string.IsNullOrEmpty(c.cell.Formula)))
        {
            var result = _formulaParser.Evaluate(cellObject.cell.Formula);
            cellObject.value.Value = result.result;

            if (result.errorMessage is not null)
            {
                errorMessage = result.errorMessage;
            }
        }

        return errorMessage;
    }

    public List<(ExcelCell cell, CellValue value, string name)> GetAllCells()
    {
        return _cells.Values.ToList();
    }

    public bool CellExists(string cellName)
    {
        return _cells.ContainsKey(cellName.ToUpper());
    }
}