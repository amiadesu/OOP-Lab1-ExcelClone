using ExcelClone.Components;

namespace ExcelClone.Services
{
    public interface IFormulaParserService
    {
        string Evaluate(string formula, string currentCell, Spreadsheet spreadsheet);
    }
}