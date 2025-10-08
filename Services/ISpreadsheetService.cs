namespace ExcelClone.Services;

public interface ISpreadsheetService
{
    public string? UpdateCellFormula(string cellReference, string formula);
}