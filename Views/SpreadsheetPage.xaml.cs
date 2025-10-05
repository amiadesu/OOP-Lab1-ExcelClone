using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ExcelClone.Components;
using ExcelClone.Services;
using System.IO;
using ExcelClone.FileSystem;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;

namespace ExcelClone.Views;

public partial class SpreadsheetPage : ContentPage {
    private const int CELL_WIDTH = 100;
    private const int CELL_HEIGHT = 25;
    private bool _isScrolling = false;
    private int _currentColumns = 0;
    private int _currentRows = 0;
    private string _fileName = "result.table";
    
    private Spreadsheet _spreadsheet;
    private ExcelCell _activeCell;
    private Dictionary<string, Label> _cellControls = new Dictionary<string, Label>();
    private IFormulaParserService _formulaParserService;
    private ICellNameService _cellNameService;

    public SpreadsheetPage()
    {
        InitializeComponent();
        _formulaParserService = new FormulaParserService();
        _cellNameService = new CellNameService();
        GenerateExcelGridAsync(true);
    }

    public SpreadsheetPage(string tablePath, string tableFileName)
    {
        InitializeComponent();
        _formulaParserService = new FormulaParserService();
        _cellNameService = new CellNameService();
        _spreadsheet = FileLoadService.Load(tablePath, _formulaParserService, _cellNameService);
        _currentColumns = _spreadsheet.Columns;
        _currentRows = _spreadsheet.Rows;
        _fileName = tableFileName;
        GenerateExcelGridAsync(false);
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        await GenerateExcelGridAsync();
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new StartingPage());
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string result = await FileSaveService.Save(_spreadsheet, _fileName);
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

    private void UpdateDimensionsInputs()
    {
        ColumnsEntry.Text = _currentColumns.ToString();
        RowsEntry.Text = _currentRows.ToString();
    }

    private void GetDimensionsAndGenerateSpreadsheet()
    {
        if (!int.TryParse(ColumnsEntry.Text, out int columns) || !int.TryParse(RowsEntry.Text, out int rows))
        {
            ShowError("Please enter valid numbers.");
            return;
        }

        if (columns <= 0 || rows <= 0)
        {
            ShowError("Both dimensions must be greater than 0.");
            return;
        }

        _currentColumns = columns;
        _currentRows = rows;

        _spreadsheet = new Spreadsheet(_currentColumns, _currentRows, _formulaParserService, _cellNameService);
    }

    private async Task GenerateExcelGridAsync(bool getNewDimensions = true)
    {
        var stopwatch = Stopwatch.StartNew();

        // Clear previous data
        ColumnHeadersLayout.Children.Clear();
        RowHeadersLayout.Children.Clear();
        DataGridLayout.Children.Clear();
        _cellControls.Clear();

        StatusLabel.Text = "Generating grid...";

        if (getNewDimensions)
        {
            GetDimensionsAndGenerateSpreadsheet();
        }
        else
        {
            _currentColumns = _spreadsheet.Columns;
            _currentRows = _spreadsheet.Rows;
            UpdateDimensionsInputs();
        }

        try
        {
            // Create headers
            CreateColumnHeaders(_currentColumns);
            CreateRowHeaders(_currentRows);

            // Update UI immediately
            GridContainer.IsVisible = true;
            FormulaBarContainer.IsVisible = true;
            StatusLabel.Text = $"Creating {_currentColumns}x{_currentRows} grid...";

            // Create data cells
            await CreateDataCellsAsync(_currentColumns, _currentRows);

            stopwatch.Stop();
            StatusLabel.Text = $"✅ Grid ready! {_currentColumns} columns x {_currentRows} rows in {stopwatch.ElapsedMilliseconds}ms";
            StatusLabel.TextColor = Colors.Green;
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }

    private void CreateColumnHeaders(int columns)
    {
        for (int col = 0; col < columns; col++)
        {
            string columnName = _cellNameService.GetColumnName(col); // Use CellNameService
            var header = new Label
            {
                Text = columnName,
                WidthRequest = CELL_WIDTH,
                HeightRequest = 25,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#217346"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            
            ColumnHeadersLayout.Children.Add(header);
        }
    }

    private void CreateRowHeaders(int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            string rowName = _cellNameService.GetRowName(row); // Use CellNameService
            var header = new Label
            {
                Text = rowName,
                WidthRequest = 40,
                HeightRequest = CELL_HEIGHT,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#4a86e8"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            
            RowHeadersLayout.Children.Add(header);
        }
    }

    private async Task CreateDataCellsAsync(int columns, int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                string cellAddress = _cellNameService.GetCellName(col, row); // Use CellNameService
                
                // Get cell from spreadsheet
                var excelCell = _spreadsheet.GetCell(cellAddress);

                // Create UI control
                var cellControl = new Label
                {
                    Text = excelCell.Value.ToString(),
                    WidthRequest = CELL_WIDTH,
                    HeightRequest = CELL_HEIGHT,
                    FontSize = 10,
                    TextColor = Colors.Black,
                    BackgroundColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                    Padding = new Thickness(2, 0, 2, 0)
                };

                // Add tap gesture for cell selection
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnCellTapped(cellAddress, cellControl);
                cellControl.GestureRecognizers.Add(tapGesture);

                DataGridLayout.Children.Add(cellControl);
                AbsoluteLayout.SetLayoutBounds(cellControl, new Rect(col * CELL_WIDTH, row * CELL_HEIGHT, CELL_WIDTH, CELL_HEIGHT));

                _cellControls[cellAddress] = cellControl;

                // Yield control periodically to keep UI responsive
                if ((row * columns + col) % 500 == 0)
                {
                    await Task.Delay(1);
                }
            }
        }
    }

    private void OnCellTapped(string cellAddress, Label cellControl)
    {
        // Clear previous active cell styling
        if (_activeCell != null && _cellControls.ContainsKey(_activeCell.Name))
        {
            _cellControls[_activeCell.Name].BackgroundColor = Colors.White;
        }

        // Set new active cell
        _activeCell = _spreadsheet.GetCell(cellAddress);
        
        // Update active cell styling
        cellControl.BackgroundColor = Color.FromArgb("#e3f2fd"); // Light blue selection
        
        // Update formula bar
        ActiveCellLabel.Text = cellAddress;
        FormulaEntry.Text = _activeCell.Formula;
        FormulaEntry.Focus();
    }

    private void OnFormulaEntryCompleted(object sender, EventArgs e)
    {
        ApplyCellValue();
    }

    private void ApplyCellValue()
    {
        if (_activeCell == null) return;

        string inputValue = FormulaEntry.Text?.Trim() ?? "";

        string? errorMessage = "";
        // Use spreadsheet to set the value (handles formulas and dependencies)
        _spreadsheet.SetCellValue(_activeCell.Name, inputValue, ref errorMessage);

        if (errorMessage is not null && errorMessage.Length > 0)
        {
            DisplayAlert(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                errorMessage,
                DataProcessor.FormatResource(
                    AppResources.OK
                )
            );
        }

        // Update ALL cells in case of dependencies
        UpdateAllCellDisplays();

        // Update status
        StatusLabel.Text = $"Cell {_activeCell.Name} updated";
        StatusLabel.TextColor = Colors.Green;
    }

    private void UpdateAllCellDisplays()
    {
        foreach (var cell in _spreadsheet.GetAllCells())
        {
            if (_cellControls.ContainsKey(cell.Name))
            {
                var cellControl = _cellControls[cell.Name];
                cellControl.Text = cell.Value.ToString();
                
                // Color code based on content type
                if (!string.IsNullOrEmpty(cell.Formula))
                {
                    cellControl.TextColor = Color.FromArgb("#0b5394"); // Blue for formulas
                }
                else if (!string.IsNullOrEmpty(cell.Value))
                {
                    cellControl.TextColor = Colors.Black; // Black for values
                }
                else
                {
                    cellControl.TextColor = Colors.Gray; // Gray for empty
                }
            }
        }
    }

    // Synchronized scrolling
    private void OnDataGridScrolled(object sender, ScrolledEventArgs e)
    {
        if (_isScrolling) return;
        _isScrolling = true;
        
        ColumnHeadersScroll.ScrollToAsync(e.ScrollX, 0, false);
        RowHeadersScroll.ScrollToAsync(0, e.ScrollY, false);
        
        _isScrolling = false;
    }

    private void OnColumnHeadersScrolled(object sender, ScrolledEventArgs e)
    {
        if (_isScrolling) return;
        _isScrolling = true;
        
        DataGridScroll.ScrollToAsync(e.ScrollX, DataGridScroll.ScrollY, false);
        _isScrolling = false;
    }

    private void OnRowHeadersScrolled(object sender, ScrolledEventArgs e)
    {
        if (_isScrolling) return;
        _isScrolling = true;
        
        DataGridScroll.ScrollToAsync(DataGridScroll.ScrollX, e.ScrollY, false);
        _isScrolling = false;
    }

    private void ShowError(string message)
    {
        StatusLabel.Text = $"❌ {message}";
        StatusLabel.TextColor = Colors.Red;
        GridContainer.IsVisible = false;
        FormulaBarContainer.IsVisible = false;
    }
}