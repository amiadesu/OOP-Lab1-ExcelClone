using Microsoft.Maui.Controls;
using System;
using ExcelClone.FileSystem;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Components.CellStorage;
using ExcelClone.Services;
using ExcelClone.Services.GoogleDrive;
using System.Threading.Tasks;

namespace ExcelClone.Views;

public partial class GoogleDriveSavePage : ContentPage 
{
    readonly TableFileService _tableFileService = new();
    private readonly GoogleDriveService _googleDriveService = new();
    private bool _initialized = false;
    private readonly ICellStorage _cellStorage;
    private readonly ICellNameService _cellNameService;
    private bool _authorized => _googleDriveService.IsSignedIn;

    private string _fullFileName = "";
    private string _fileName
    {
        get
        {
            if (_fullFileName.EndsWith(".table"))
                return _fullFileName.Substring(0, _fullFileName.Length - 6); // 6 = length of ".table"
            return _fullFileName;
        }
        set
        {
            if (!value.EndsWith(".table"))
                _fullFileName = value + ".table";
            else
                _fullFileName = value;
        }
    }

    public GoogleDriveSavePage(ICellStorage spreadsheet, ICellNameService cellNameService, string fileName)
    {
        _cellStorage = spreadsheet;
        _cellNameService = cellNameService;
        _fileName = fileName;

        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;

        FileNameEntry.Text = _fileName;

        try
        {
            await _googleDriveService.Init();
            UpdateButtons();
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

    private async void OnReturnClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new SpreadsheetPage(_cellStorage, _cellNameService, _fullFileName));
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        await Authorize();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _fileName = FileNameEntry.Text;
        var result = await _tableFileService.SaveToGoogleDrive(_cellStorage, _googleDriveService, _fullFileName);

        await DisplayAlert(
            DataProcessor.FormatResource(
                AppResources.SavingResult
            ),
            result,
            DataProcessor.FormatResource(
                AppResources.OK
            )
        );
    }
    
    private async Task Authorize()
    {
        try
        {
            if (!_authorized)
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
            SaveButton.IsVisible = true;
        }
        else
        {
            SignInButton.Text = DataProcessor.FormatResource(
                AppResources.SignIn
            );
            SaveButton.IsVisible = false;
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