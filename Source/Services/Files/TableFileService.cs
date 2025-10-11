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
using ExcelClone.Services.GoogleDrive;

namespace ExcelClone.FileSystem;

public static class TableFileService
{
    public static string ConstructFileFromCellStorage(ICellStorageReader spreadsheet)
    {
        StringBuilder result = new StringBuilder();

        int columns = spreadsheet.GetColumns();
        int rows = spreadsheet.GetRows();

        // Write header
        result.AppendLine($"{columns} {rows}");

        int totalCells = columns * rows;
        for (int i = 0; i < totalCells; i++)
        {
            int col = i / rows;
            int row = i % rows;

            string cellName = spreadsheet.GetCellName(col, row);
            var formula = spreadsheet.GetCellFormula(cellName);
            result.AppendLine(formula);
        }

        return result.ToString();
    }

    public static async Task<string> Save(ICellStorageReader spreadsheet, string fileName = "result.table")
    {
        var content = ConstructFileFromCellStorage(spreadsheet);

        var fileSaverResult = await PickAndSaveFile(content, fileName);
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

    public static async Task<string> SaveToGoogleDrive(ICellStorageReader spreadsheet,
        IGoogleDriveService googleDriveService, string fileName = "result.table")
    {
        try
        {
            var content = ConstructFileFromCellStorage(spreadsheet);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var file = await googleDriveService.UploadFileAsync(fileName, stream, "text/plain");

            return DataProcessor.FormatResource(
                AppResources.FileSavedSuccessfully,
                ("Path", $"Google Drive, {file.Name} (id: {file.Id})")
            );
        }
        catch (Exception e)
        {
            return DataProcessor.FormatResource(
                AppResources.FileSavingError,
                ("Error", e.Message)
            );
        }
    }

    public static Spreadsheet LoadFromPath(string path, ICellNameService nameService)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        using var reader = new StreamReader(path);

        return LoadFile(reader, nameService);
    }

    public static Spreadsheet LoadFromContentString(string content, ICellNameService nameService)
    {
        using var reader = new StreamReader(
            new MemoryStream(Encoding.UTF8.GetBytes(content))
        );

        return LoadFile(reader, nameService);
    }

    public static async Task<Spreadsheet> LoadFromGoogleDrive(string fileId,
        IGoogleDriveService googleDriveService, ICellNameService nameService)
    {
        var content = await googleDriveService.DownloadFileAsync(fileId);

        return LoadFromContentString(content, nameService);
    }

    private static Spreadsheet LoadFile(StreamReader reader, ICellNameService nameService)
    {
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

        var spreadsheet = new Spreadsheet(nameService);
        spreadsheet.CreateNewCellStorage(columns, rows);

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
                spreadsheet.SetCellFormula(cellName, line);
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
