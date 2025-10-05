using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using ExcelClone.Components;
using ExcelClone.Resources.Localization;
using ExcelClone.Utils;

namespace ExcelClone.FileSystem;

public static class FileSaveService
{
    public static async Task<string> Save(Spreadsheet spreadsheet, string fileName = "result.table")
    {
        StringBuilder result = new StringBuilder();

        // Write header
        result.AppendLine($"{spreadsheet.Columns} {spreadsheet.Rows}");

        int totalCells = spreadsheet.Columns * spreadsheet.Rows;
        for (int i = 0; i < totalCells; i++)
        {
            int col = i / spreadsheet.Rows;
            int row = i % spreadsheet.Rows;

            string cellName = spreadsheet.cellNameService.GetCellName(col, row);
            var cell = spreadsheet.GetCell(cellName);
            string formula = cell?.Formula ?? string.Empty;
            result.AppendLine(formula);
        }

        var fileSaverResult = await PickAndSaveFile(result.ToString(), fileName);
        if (fileSaverResult.IsSuccessful)
        {
            return DataProcessor.FormatResource(
                AppResources.FileSavedSuccessfully,
                ("Path", fileSaverResult.FilePath)
            );
        }
        return DataProcessor.FormatResource(
            AppResources.FileSavingError,
            ("Error", fileSaverResult.Exception.Message)
        );
    }

    public static async Task<FileSaverResult> PickAndSaveFile(string data, string fileName = "result.table")
    {
        using var stream = new MemoryStream(Encoding.Default.GetBytes(data));
        var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
        return fileSaverResult;
    }
}