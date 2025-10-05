using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using ExcelClone.FileSystem;
using System.Diagnostics;

namespace ExcelClone.Views;

public partial class HelpPage : ContentPage {

    public HelpPage()
    {
        InitializeComponent();
    }

    private async void OnHomePageClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new StartingPage());
    }
    
    private async void OnOpenSpreadsheetClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new SpreadsheetPage());
    }

    private async void OnOpenFilesClicked(object sender, EventArgs e)
    {
        var result = await FilePickService.PickTable("Pick table");
        if (result is not null)
        {
            await Shell.Current.Navigation.PushAsync(new SpreadsheetPage(result.FullPath, result.FileName));
        }
    }
}