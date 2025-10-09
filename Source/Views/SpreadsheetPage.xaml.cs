using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ExcelClone.Components;
using ExcelClone.Services;
using ExcelClone.FileSystem;
using ExcelClone.Utils;
using ExcelClone.Resources.Localization;
using ExcelClone.Constants;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Parsers;

namespace ExcelClone.Views;

public partial class SpreadsheetPage : ContentPage
{
    private bool _isScrolling = false;
    private int _currentColumns = 0;
    private int _currentRows = 0;
    private readonly string _fileName = Literals.defaultFileName;

    private string _activeCellName = "";
    private readonly Dictionary<string, Label> _cellControls = new Dictionary<string, Label>();
    private readonly ICellStorage _spreadsheet;
    private readonly IDependencyTree _dependencyTree;
    private readonly IFormulaTokenizer _formulaTokenizer;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IParser _parser;
    private readonly IFormulaParserService _formulaParser;
    private readonly ICellNameService _cellNameService;
    private readonly ISpreadsheetService _spreadsheetService;

    public SpreadsheetPage()
    {
        InitializeComponent();

        _cellNameService = new CellNameService();
        _spreadsheet = new Spreadsheet(_cellNameService);

        _dependencyTree = new DependencyTree();

        _parser = new Parser(_spreadsheet);

        _formulaTokenizer = new FormulaTokenizer();
        _formulaEvaluator = new FormulaEvaluator(_parser);

        _formulaParser = new FormulaParserService(_formulaTokenizer, _formulaEvaluator);

        _spreadsheetService = new SpreadsheetService(_spreadsheet, _dependencyTree, _formulaParser);

        GenerateExcelGrid(true);
    }

    public SpreadsheetPage(string tablePath, string tableFileName)
    {
        InitializeComponent();

        _cellNameService = new CellNameService();
        _spreadsheet = TableFileService.Load(tablePath, _cellNameService);

        _dependencyTree = new DependencyTree();

        _parser = new Parser(_spreadsheet);

        _formulaTokenizer = new FormulaTokenizer();
        _formulaEvaluator = new FormulaEvaluator(_parser);

        _formulaParser = new FormulaParserService(_formulaTokenizer, _formulaEvaluator);

        _spreadsheetService = new SpreadsheetService(_spreadsheet, _dependencyTree, _formulaParser);

        _currentColumns = _spreadsheet.GetColumns();
        _currentRows = _spreadsheet.GetRows();
        _fileName = tableFileName;

        RecalculateAllCells();

        GenerateExcelGrid(false);

        UpdateAllCellDisplays();
    }

    private void OnGenerateClicked(object sender, EventArgs e)
    {
        GenerateExcelGrid();
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        bool result = await OpenConfirmation();
        if (result)
            await Shell.Current.Navigation.PushAsync(new StartingPage());
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        bool result = await OpenConfirmation();
        if (result)
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

        _spreadsheet.CreateNewCellStorage(_currentColumns, _currentRows);
    }

    private void GenerateExcelGrid(bool getNewDimensions = true)
    {
        if (getNewDimensions)
        {
            GetDimensionsAndGenerateSpreadsheet();
        }
        else
        {
            _currentColumns = _spreadsheet.GetColumns();
            _currentRows = _spreadsheet.GetRows();
            UpdateDimensionsInputs();
        }

        GenerateWithClock();
    }

    private void ClearEverything()
    {
        ColumnHeadersLayout.Children.Clear();
        RowHeadersLayout.Children.Clear();
        DataGridLayout.Children.Clear();
        _cellControls.Clear();
    }

    private void GenerateGridWithErrorFetching()
    {
        try
        {
            ClearEverything();

            CreateColumnHeaders(_currentColumns);
            CreateRowHeaders(_currentRows);

            CreateDataCells(_currentColumns, _currentRows);
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
    
    private void GenerateWithClock()
    {
        var stopwatch = Stopwatch.StartNew();

        StatusLabel.Text = DataProcessor.FormatResource(
            AppResources.CreatingGrid,
            ("Columns", _currentColumns),
            ("Rows", _currentRows)
        );

        GenerateGridWithErrorFetching();

        stopwatch.Stop();

        StatusLabel.Text = DataProcessor.FormatResource(
            AppResources.GridCreated,
            ("Columns", _currentColumns),
            ("Rows", _currentRows),
            ("TimeMs", stopwatch.ElapsedMilliseconds)
        );
        StatusLabel.TextColor = Colors.Green;
    }

    private void CreateColumnHeaders(int columns)
    {
        for (int col = 0; col < columns; col++)
        {
            string columnName = _cellNameService.GetColumnName(col);

            UIGenerator.GenerateColumnHeader(columnName, ref ColumnHeadersLayout);
        }
    }

    private void CreateRowHeaders(int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            string rowName = _cellNameService.GetRowName(row);

            UIGenerator.GenerateRowHeader(rowName, ref RowHeadersLayout);
        }
    }

    private void CreateDataCells(int columns, int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                string cellAddress = _cellNameService.GetCellName(col, row);

                var cellValue = _spreadsheet.GetCellValue(cellAddress);

                if (cellValue is null)
                {
                    Trace.TraceError($"Cell {cellAddress} is unexpectedly empty");
                    continue;
                }

                var cell = UIGenerator.GenerateCell(cellValue, col, row, ref DataGridLayout);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnCellTapped(cellAddress, cell);
                cell.GestureRecognizers.Add(tapGesture);

                _cellControls[cellAddress] = cell;
            }
        }
    }

    private void OnCellTapped(string cellAddress, Label cellControl)
    {
        if (!string.IsNullOrEmpty(_activeCellName) && _cellControls.ContainsKey(_activeCellName))
        {
            _cellControls[_activeCellName].BackgroundColor = ColorConstants.cellBackgroundColor;
        }

        _activeCellName = cellAddress;

        var cellFormula = _spreadsheet.GetCellFormula(_activeCellName);

        if (cellFormula is null)
        {
            Trace.TraceError($"Cell {_activeCellName} is unexpectedly empty");
            return;
        }

        cellControl.BackgroundColor = ColorConstants.activeCellBackgroundColor;

        ActiveCellLabel.Text = cellAddress;
        FormulaEntry.Text = cellFormula;
        FormulaEntry.Focus();
    }

    private void OnFormulaEntryCompleted(object sender, EventArgs e)
    {
        ApplyCellFormula();
    }

    private void ApplyCellFormula()
    {
        if (string.IsNullOrEmpty(_activeCellName)) return;

        string formula = FormulaEntry.Text?.Trim() ?? "";

        string? errorMessage = _spreadsheetService.UpdateCellFormula(_activeCellName, formula);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            ShowError(
                DataProcessor.FormatResource(
                    AppResources.Error
                ),
                errorMessage
            );
        }

        UpdateAllCellDisplays();
    }

    private void RecalculateAllCells()
    {
        foreach (var cellName in _cellNameService.CellNames(_currentColumns, _currentRows))
        {
            var formula = _spreadsheet.GetCellFormula(cellName);

            _spreadsheetService.UpdateCellFormula(cellName, formula);
        }
    }

    private void UpdateAllCellDisplays()
    {
        foreach (var cellName in _cellNameService.CellNames(_currentColumns, _currentRows))
        {
            var cellControl = _cellControls[cellName];

            UpdateCellText(ref cellControl, cellName);
            UpdateCellColor(ref cellControl, cellName);
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

    private void UpdateCellText(ref Label cellLabel, string cellName)
    {
        if (DisplayModeSwitch.IsToggled)
        {
            cellLabel.Text = _spreadsheet.GetCellFormula(cellName);
        }
        else
        {
            cellLabel.Text = _spreadsheet.GetCellDisplayValue(cellName);
        }
    }

    private void UpdateCellColor(ref Label cellLabel, string cellName)
    {
        var cellFormula = _spreadsheet.GetCellFormula(cellName);
        var cellValue = _spreadsheet.GetCellValue(cellName);

        if (cellFormula is null || cellValue is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(cellFormula))
        {
            cellLabel.TextColor = ColorConstants.formulaColor;
        }
        else if (!string.IsNullOrEmpty(cellValue.Value))
        {
            cellLabel.TextColor = ColorConstants.valueColor;
        }
        else
        {
            cellLabel.TextColor = ColorConstants.emptyColor;
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

    private async Task<bool> OpenConfirmation()
    {
        bool result = await DisplayAlert(
            DataProcessor.FormatResource(
                AppResources.ActionConfirmation
            ),
            DataProcessor.FormatResource(
                AppResources.CurrentDataWillBeLost
            ),
            DataProcessor.FormatResource(
                AppResources.Yes
            ),
            DataProcessor.FormatResource(
                AppResources.No
            )
        );

        return result;
    }
}