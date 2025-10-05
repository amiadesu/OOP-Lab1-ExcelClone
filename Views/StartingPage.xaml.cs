using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using ExcelClone.FileSystem;
using System.Diagnostics;

namespace ExcelClone.Views;

public partial class StartingPage : ContentPage {

    public StartingPage()
    {
        InitializeComponent();
    }
    
    private async void OnOpenSpreadsheetClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SpreadsheetPage));
    }

    private async void OnOpenFilesClicked(object sender, EventArgs e)
    {
        var result = await FilePickService.PickTable("Pick table");
        if (result is not null)
        {
            await Shell.Current.Navigation.PushModalAsync(new SpreadsheetPage(result.FullPath, result.FileName));
        }
    }
}