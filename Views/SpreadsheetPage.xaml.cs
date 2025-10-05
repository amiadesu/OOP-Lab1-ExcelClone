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
using ExcelClone.Constants;

namespace ExcelClone.Views;

public partial class SpreadsheetPage : ContentPage {
    private bool _isScrolling = false;
    private int _currentColumns = 0;
    private int _currentRows = 0;
    private readonly string _fileName = Literals.defaultFileName;
    
    private Spreadsheet _spreadsheet;
    private ExcelCell _activeCell;
    private readonly Dictionary<string, Label> _cellControls = new Dictionary<string, Label>();
    private readonly IFormulaParserService _formulaParserService;
    private readonly ICellNameService _cellNameService;

    public SpreadsheetPage()
    {
        InitializeComponent();
        _formulaParserService = new FormulaParserService();
        _cellNameService = new CellNameService();
        GenerateExcelGrid(true);
    }

    public SpreadsheetPage(string tablePath, string tableFileName)
    {
        InitializeComponent();
        _formulaParserService = new FormulaParserService();
        _cellNameService = new CellNameService();
        _spreadsheet = TableFileService.Load(tablePath, _formulaParserService, _cellNameService);
        _currentColumns = _spreadsheet.Columns;
        _currentRows = _spreadsheet.Rows;
        _fileName = tableFileName;
        GenerateExcelGrid(false);
    }

    private void OnGenerateClicked(object sender, EventArgs e)
    {
        GenerateExcelGrid();
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new StartingPage());
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new HelpPage());
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string result = await TableFileService.Save(_spreadsheet, _fileName);
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
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                DataProcessor.FormatResource(
                    AppResources.EnterValidNumbers
                )
            );
            return;
        }

        if (columns <= 0 || rows <= 0)
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                DataProcessor.FormatResource(
                    AppResources.DimensionsMustBeGreaterThan0
                )
            );
            return;
        }

        _currentColumns = columns;
        _currentRows = rows;

        _spreadsheet = new Spreadsheet(_currentColumns, _currentRows, _formulaParserService, _cellNameService);
    }

    private void GenerateExcelGrid(bool getNewDimensions = true)
    {
        var stopwatch = Stopwatch.StartNew();

        ColumnHeadersLayout.Children.Clear();
        RowHeadersLayout.Children.Clear();
        DataGridLayout.Children.Clear();
        _cellControls.Clear();

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
            CreateColumnHeaders(_currentColumns);
            CreateRowHeaders(_currentRows);

            StatusLabel.Text = DataProcessor.FormatResource(
                AppResources.CreatingGrid,
                ("Columns", _currentColumns),
                ("Rows", _currentRows)
            );

            CreateDataCells(_currentColumns, _currentRows);

            stopwatch.Stop();
            StatusLabel.Text = DataProcessor.FormatResource(
                AppResources.GridCreated,
                ("Columns", _currentColumns),
                ("Rows", _currentRows),
                ("TimeMs", stopwatch.ElapsedMilliseconds)
            );
            StatusLabel.TextColor = Colors.Green;
        }
        catch (Exception ex)
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                ex.Message
            );
        }
    }

    private void CreateColumnHeaders(int columns)
    {
        for (int col = 0; col < columns; col++)
        {
            string columnName = _cellNameService.GetColumnName(col);
            
            var header = UIGenerator.GenerateColumnHeader(columnName, ref ColumnHeadersLayout);
        }
    }

    private void CreateRowHeaders(int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            string rowName = _cellNameService.GetRowName(row);

            var header = UIGenerator.GenerateRowHeader(rowName, ref RowHeadersLayout);
        }
    }

    private void CreateDataCells(int columns, int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                string cellAddress = _cellNameService.GetCellName(col, row);
                
                var excelCell = _spreadsheet.GetCell(cellAddress);

                var cell = UIGenerator.GenerateCell(excelCell, col, row, ref DataGridLayout);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnCellTapped(cellAddress, cell);
                cell.GestureRecognizers.Add(tapGesture);

                _cellControls[cellAddress] = cell;
            }
        }
    }

    private void OnCellTapped(string cellAddress, Label cellControl)
    {
        if (_activeCell != null && _cellControls.ContainsKey(_activeCell.Name))
        {
            _cellControls[_activeCell.Name].BackgroundColor = ColorConstants.cellBackgroundColor;
        }

        _activeCell = _spreadsheet.GetCell(cellAddress);
        
        cellControl.BackgroundColor = ColorConstants.activeCellBackgroundColor;
        
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
        
        _spreadsheet.SetCellValue(_activeCell.Name, inputValue, ref errorMessage);

        if (errorMessage is not null && errorMessage.Length > 0)
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                errorMessage
            );
        }

        // Update ALL cells in case of dependencies
        UpdateAllCellDisplays();
    }

    private void UpdateAllCellDisplays()
    {
        foreach (var cell in _spreadsheet.GetAllCells())
        {
            if (_cellControls.ContainsKey(cell.Name))
            {
                var cellControl = _cellControls[cell.Name];

                UpdateCellText(ref cellControl, cell);                
                
                if (!string.IsNullOrEmpty(cell.Formula))
                {
                    cellControl.TextColor = ColorConstants.formulaColor;
                }
                else if (!string.IsNullOrEmpty(cell.Value))
                {
                    cellControl.TextColor = ColorConstants.valueColor;
                }
                else
                {
                    cellControl.TextColor = ColorConstants.emptyColor;
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

    private void OnDisplayModeToggle(object sender, ToggledEventArgs e)
    {
        UpdateAllCellDisplays();
    }

    private void UpdateCellText(ref Label cellObject, ExcelCell cellData)
    {
        if (DisplayModeSwitch.IsToggled)
        {
            cellObject.Text = cellData.Formula;
        }
        else
        {
            cellObject.Text = cellData.Value.ToString();
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