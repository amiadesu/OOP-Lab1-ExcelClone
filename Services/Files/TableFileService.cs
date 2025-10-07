using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;
using ExcelClone.Components;
using ExcelClone.Resources.Localization;
using ExcelClone.Services;
using ExcelClone.Utils;

namespace ExcelClone.FileSystem;

public static class TableFileService
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

    public static Spreadsheet Load(string path, IFormulaParserService parser, ICellNameService nameService)
    {
        using var reader = new StreamReader(path);

        // Read header
        string? header = reader.ReadLine();
        if (header == null)
        {
            throw new InvalidDataException(DataProcessor.FormatResource(
                AppResources.EmptyFile
            ));
        }

        var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int columns) ||
            !int.TryParse(parts[1], out int rows))
        {
            throw new InvalidDataException(DataProcessor.FormatResource(
                AppResources.InvalidFileHeader
            ));
        }

        var spreadsheet = new Spreadsheet(columns, rows, parser, nameService);

        // Read cell formulas
        int i = 0;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            int col = i / rows;
            int row = i % rows;
            string cellName = nameService.GetCellName(col, row);

            if (!string.IsNullOrEmpty(line))
            {
                string? errorMessage = "";
                spreadsheet.SetCellValue(cellName, line, ref errorMessage);
            }

            i++;
        }

        return spreadsheet;
    }

    public static async Task<(FileResult? result, string? errorMessage)> PickTable(string pickTitle)
    {
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".table" } }, // file extension
                    { DevicePlatform.macOS, new[] { "table" } }, // UTType values
                });

        PickOptions options = new()
        {
            PickerTitle = pickTitle,
            FileTypes = customFileType,
        };

        return await PickAndShow(options);
    }
    public static async Task<(FileResult? result, string? errorMessage)> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            
            return (result, "");
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
            return (null, ex.Message);
        }
    }

    public static async Task<FileSaverResult> PickAndSaveFile(string data, string fileName = "result.table")
    {
        using var stream = new MemoryStream(Encoding.Default.GetBytes(data));
        var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
        return fileSaverResult;
    }
}
