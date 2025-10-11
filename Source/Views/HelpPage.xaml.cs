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

    private async void OnHomePageClicked(object sender, EventArgs e)
    {
        SetLoading(true);
        await Shell.Current.Navigation.PushAsync(new StartingPage());
        SetLoading(false);
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }
}