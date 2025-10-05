using System.IO;
using ExcelClone.Components;

namespace ExcelClone.FileSystem;

public static class FileSaveService
{
    public static void Save(Spreadsheet spreadsheet, string path)
    {
        using var writer = new StreamWriter(path);

        // Write header
        writer.WriteLine($"{spreadsheet.Columns} {spreadsheet.Rows}");

        int totalCells = spreadsheet.Columns * spreadsheet.Rows;
        for (int i = 0; i < totalCells; i++)
        {
            int col = i / spreadsheet.Rows;
            int row = i % spreadsheet.Rows;

            string cellName = spreadsheet.cellNameService.GetCellName(col, row);
            var cell = spreadsheet.GetCell(cellName);
            string formula = cell?.Formula ?? string.Empty;
            writer.WriteLine(formula);
        }
    }
}