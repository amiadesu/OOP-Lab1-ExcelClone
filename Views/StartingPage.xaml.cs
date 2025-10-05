using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using ExcelClone.FileSystem;
using System.Diagnostics;
using System.Threading.Tasks;

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
        var result = await FilePickService.PickTable("Pick table");
        if (result is not null)
        {
            await OpenFile(result.FileName, result.FullPath);
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
                var button = new Button
                {
                    Text = entry.FileName,
                    BackgroundColor = Colors.Gray,
                    HeightRequest = 40
                };
                button.Clicked += (async (sender, e) => await OpenFile(entry.FileName, entry.FilePath));

                RecentItemsContainer.Children.Add(button);
            }
        }
    }
}