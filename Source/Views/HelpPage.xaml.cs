using Microsoft.Maui.Controls;
using System;
using ExcelClone.FileSystem;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;

namespace ExcelClone.Views;

public partial class HelpPage : ContentPage 
{
    public HelpPage()
    {
        InitializeComponent();
    }

    private static async void OnHomePageClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new StartingPage());
    }
    
    private static async void OnOpenSpreadsheetClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new SpreadsheetPage());
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
            await Shell.Current.Navigation.PushAsync(new SpreadsheetPage(result.result.FullPath, result.result.FileName));
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
}