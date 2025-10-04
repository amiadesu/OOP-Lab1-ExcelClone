using ExcelClone.Components;
using ExcelClone.Values;

namespace ExcelClone.Services
{
    public interface IFormulaParserService
    {
        CellValue Evaluate(string formula, string currentCell, Spreadsheet spreadsheet);
    }
}