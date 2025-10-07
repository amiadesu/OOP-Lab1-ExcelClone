using System.Collections.Generic;
using System.Linq;
using ExcelClone.Services;
using ExcelClone.Constants;
using ExcelClone.Values;

namespace ExcelClone.Components
{
    public class Spreadsheet
    {
        private readonly Dictionary<string, ExcelCell> _cells;
        private readonly IFormulaParserService _formulaParser;
        public readonly ICellNameService cellNameService;
        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public Spreadsheet(int columns, int rows, IFormulaParserService formulaParser, ICellNameService cellNameService)
        {
            this.cellNameService = cellNameService;
            Columns = columns;
            Rows = rows;
            _formulaParser = formulaParser;
            _cells = new Dictionary<string, ExcelCell>();
            InitializeCells();
        }

        private void InitializeCells()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    string cellName = cellNameService.GetCellName(col, row);
                    _cells[cellName] = new ExcelCell(cellName);
                }
            }
        }

        public ExcelCell? GetCell(string cellName)
        {
            return _cells.ContainsKey(cellName.ToUpper()) ? _cells[cellName.ToUpper()] : null;
        }

        public void SetCellValue(string cellName, string value, ref string? errorMessage)
        {
            if (!_cells.ContainsKey(cellName.ToUpper()))
                return;

            var cell = _cells[cellName.ToUpper()];
            
            if (value.StartsWith(Literals.prefix))
            {
                cell.Formula = value;
                cell.Value = _formulaParser.Evaluate(value, cellName, this, ref errorMessage);
            }
            else
            {
                cell.Formula = value;
                cell.Value.Value = value;
            }

            // Recalculate cells
            RecalculateCells();
        }

        public string GetCellDisplayValue(string cellName)
        {
            var cell = GetCell(cellName);
            return cell?.Value.ToString() ?? Literals.refErrorMessage;
        }

        public CellValue? GetCellRealValue(string cellName)
        {
            var cell = GetCell(cellName);
            return cell?.Value ?? null;
        }

        private void RecalculateCells()
        {
            // Recalculate all formula cells
            foreach (var cell in _cells.Values)
            {
                if (!string.IsNullOrEmpty(cell.Formula))
                {
                    string? message = "";
                    cell.Value = _formulaParser.Evaluate(cell.Formula, cell.Name, this, ref message);
                }
            }
        }

        public List<ExcelCell> GetAllCells()
        {
            return _cells.Values.ToList();
        }

        public bool CellExists(string cellName)
        {
            return _cells.ContainsKey(cellName.ToUpper());
        }
    }
}