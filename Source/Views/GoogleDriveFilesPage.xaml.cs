using Microsoft.Maui.Controls;
using System;
using ExcelClone.FileSystem;
using System.Threading.Tasks;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Services.GoogleDrive;
using ExcelClone.Services;

namespace ExcelClone.Views;

public partial class GoogleDriveFilesPage : ContentPage
{
    readonly GoogleDriveService _googleDriveService = new();
    readonly CellNameService _cellNameService = new();
    private bool _initialized = false;
    private bool _authorized => _googleDriveService.IsSignedIn;

    public GoogleDriveFilesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;

        try
        {
            await _googleDriveService.Init();
            UpdateButtons();

            if (_googleDriveService.IsSignedIn)
            {
                await GenerateGoogleDriveFileObjects();
            }
        }
        catch (Exception e)
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                e.Message
            );
        }
    }

    private static async void OnHomePageClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new StartingPage());
    }

    private async Task OpenFile(string fileName, string fileId)
    {
        var spreadsheet = await TableFileService.LoadFromGoogleDrive(fileId, _googleDriveService, _cellNameService);
        await Shell.Current.Navigation.PushAsync(new SpreadsheetPage(spreadsheet, _cellNameService, fileName));
    }

    private async Task GenerateGoogleDriveFileObjects()
    {
        try
        {
            var files = await _googleDriveService.GetFiles("table");

            if (files.Count == 0)
            {
                NoGoogleDriveFilesLabel.IsVisible = true;
                GoogleDriveItemsContainer.IsVisible = false;
            }
            else
            {
                NoGoogleDriveFilesLabel.IsVisible = false;
                GoogleDriveItemsContainer.IsVisible = true;

                foreach (var file in files)
                {
                    var button = UIGenerator.GenerateRecentFileButton(file.Name, ref GoogleDriveItemsContainer);

                    button.Clicked += (async (sender, e) => await OpenFile(file.Name, file.Id));
                }
            }
        }
        catch (Exception e)
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                e.Message
            );
        }
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        await Authorize();
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        await GenerateGoogleDriveFileObjects();
    }

    private async Task Authorize()
    {
        try
        {
            if (_authorized)
            {
                await _googleDriveService.SignIn();
            }
            else
            {
                await _googleDriveService.SignOut();
            }
        }
        catch (Exception e)
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                e.Message
            );
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (_authorized)
        {
            SignInButton.Text = DataProcessor.FormatResource(
                AppResources.SignOut,
                ("Email", _googleDriveService.Email ?? "")
            );
            UpdateButton.IsVisible = true;
        }
        else
        {
            SignInButton.Text = DataProcessor.FormatResource(
                AppResources.SignIn
            );
            UpdateButton.IsVisible = false;
        }
    }

    private void ShowError(string title, string message)
    {
        DisplayAlert(
            title,
            message,
            DataProcessor.FormatResource(
                AppResources.OK
        ));
    }
}