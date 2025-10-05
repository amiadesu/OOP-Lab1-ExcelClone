using System;
using System.IO;
using ExcelClone.Services;
using ExcelClone.Components;

namespace ExcelClone.FileSystem;

public static class FileLoadService
{
    public static Spreadsheet Load(string path, IFormulaParserService parser, ICellNameService nameService)
    {
        using var reader = new StreamReader(path);

        // Read header
        string? header = reader.ReadLine();
        if (header == null)
            throw new InvalidDataException("File is empty.");

        var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int columns) ||
            !int.TryParse(parts[1], out int rows))
        {
            throw new InvalidDataException("Invalid header format.");
        }

        var spreadsheet = new Spreadsheet(columns, rows, parser, nameService);

        // Read cell formulas
        int i = 0;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            int col = i / rows;
            int row = i % rows;
            string cellName = nameService.GetCellName(col, row);

            if (!string.IsNullOrEmpty(line))
                spreadsheet.SetCellValue(cellName, line);

            i++;
        }

        return spreadsheet;
    }
}