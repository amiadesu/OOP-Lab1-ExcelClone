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
using ExcelClone.Helpers;

namespace ExcelClone.FileSystem;

public class TableFileService
{
    private readonly ITableDataFormatter _tableDataFormatter;

    public TableFileService()
    {
        _tableDataFormatter = new TableDataFormatter();
    }

    public async Task<string> SaveLocally(ICellStorageReader spreadsheet, string fileName = "result.table")
    {
        var content = _tableDataFormatter.ToTableFormat(spreadsheet);

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

    public async Task<string> SaveToGoogleDrive(ICellStorageReader spreadsheet,
        IGoogleDriveService googleDriveService, string fileName = "result.table")
    {
        try
        {
            var content = _tableDataFormatter.ToTableFormat(spreadsheet);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var file = await googleDriveService.UploadOrReplaceFileAsync(fileName, stream, "text/plain");

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

    public Spreadsheet LoadFromPath(string path, ICellNameService nameService)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        using var reader = new StreamReader(path);

        return _tableDataFormatter.FromTableFormat(reader, nameService);
    }

    public Spreadsheet LoadFromContentString(string content, ICellNameService nameService)
    {
        using var reader = new StreamReader(
            new MemoryStream(Encoding.UTF8.GetBytes(content))
        );

        return _tableDataFormatter.FromTableFormat(reader, nameService);
    }

    public async Task<Spreadsheet> LoadFromGoogleDrive(string fileId,
        IGoogleDriveService googleDriveService, ICellNameService nameService)
    {
        var content = await googleDriveService.DownloadFileAsync(fileId);

        return LoadFromContentString(content, nameService);
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
