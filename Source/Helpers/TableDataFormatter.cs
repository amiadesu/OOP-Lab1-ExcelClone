using System;
using System.IO;
using System.Text;
using ExcelClone.Components;
using ExcelClone.Resources.Localization;
using ExcelClone.Services;
using ExcelClone.Utils;

namespace ExcelClone.Helpers;

public class TableDataFormatter : ITableDataFormatter
{
    public string ToTableFormat(ICellStorageReader spreadsheet)
    {
        StringBuilder result = new StringBuilder();

        int columns = spreadsheet.GetColumns();
        int rows = spreadsheet.GetRows();

        // Write header
        result.AppendLine($"{columns} {rows}");

        int totalCells = columns * rows;
        for (int i = 0; i < totalCells; i++)
        {
            int col = i / rows;
            int row = i % rows;

            string cellName = spreadsheet.GetCellName(col, row);
            var formula = spreadsheet.GetCellFormula(cellName);
            result.AppendLine(formula);
        }

        return result.ToString();
    }

    public Spreadsheet FromTableFormat(StreamReader reader, ICellNameService nameService)
    {
        // Read header
        string? header = reader.ReadLine();
        if (header == null)
        {
            throw new InvalidDataException(DataProcessor.FormatResource(
                AppResources.EmptyFile
            ));
        }

        var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int columns) ||
            !int.TryParse(parts[1], out int rows))
        {
            throw new InvalidDataException(DataProcessor.FormatResource(
                AppResources.InvalidFileHeader
            ));
        }

        var spreadsheet = new Spreadsheet(nameService);
        spreadsheet.CreateNewCellStorage(columns, rows);

        // Read cell formulas
        int i = 0;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            int col = i / rows;
            int row = i % rows;
            string cellName = nameService.GetCellName(col, row);

            if (!string.IsNullOrEmpty(line))
            {
                spreadsheet.SetCellFormula(cellName, line);
            }

            i++;
        }

        return spreadsheet;
    }
}