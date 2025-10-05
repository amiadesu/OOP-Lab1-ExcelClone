using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace ExcelClone.FileSystem;

public static class FilePickService
{
    public static async Task<FileResult> PickTable(string pickTitle)
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
    public static async Task<FileResult> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);

            return result;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        return null;
    }
}