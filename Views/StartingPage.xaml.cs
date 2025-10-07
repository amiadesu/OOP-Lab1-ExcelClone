using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using ExcelClone.FileSystem;
using System.Diagnostics;
using System.Threading.Tasks;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;

namespace ExcelClone.Views;

public partial class StartingPage : ContentPage
{

    public StartingPage()
    {
        InitializeComponent();

        GenerateRecentFileObjects();
    }

    private async void OnOpenSpreadsheetClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new SpreadsheetPage());
    }

    private async void OnHelpPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new HelpPage());
    }

    private async void OnOpenFilesClicked(object sender, EventArgs e)
    {
        var result = await TableFileService.PickTable(
            DataProcessor.FormatResource(
                AppResources.PickTable
            )
        );
        if (result.result is not null)
        {
            await OpenFile(result.result.FileName, result.result.FullPath);
        }
        else if (!string.IsNullOrEmpty(result.errorMessage))
        {
            await DisplayAlert(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                result.errorMessage,
                DataProcessor.FormatResource(
                    AppResources.OK
                )
            );
        }
    }

    private async void OnClearRecentFilesClicked(object sender, EventArgs e)
    {
        await FileHistoryService.ClearHistoryAsync();
        await GenerateRecentFileObjects();
    }

    private async Task OpenFile(string fileName, string filePath)
    {
        await FileHistoryService.AddEntryAsync(fileName, filePath);
        await Shell.Current.Navigation.PushAsync(new SpreadsheetPage(filePath, fileName));
    }

    private async Task GenerateRecentFileObjects()
    {
        var entries = await FileHistoryService.LoadEntriesAsync();
        if (entries.Count == 0)
        {
            NoRecentFilesLabel.IsVisible = true;
            RecentItemsContainer.IsVisible = false;
        }
        else
        {
            NoRecentFilesLabel.IsVisible = false;
            RecentItemsContainer.IsVisible = true;

            foreach (var entry in entries)
            {
                var button = UIGenerator.GenerateRecentFileButton(entry.FileName, ref RecentItemsContainer);

                button.Clicked += (async (sender, e) => await OpenFile(entry.FileName, entry.FilePath));
            }
        }
    }
}