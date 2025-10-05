using System;
using System.Collections.Generic;
using System.Linq;
using ExcelClone.Resources.Localization;
using ExcelClone.Utils;

namespace ExcelClone.Services
{
    public class CellNameService : ICellNameService
    {        
        public string GetCellName(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || rowIndex < 0)
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.ColumnAndRowIndicesMustBeNonNegative
                    )
                );
            }
            
            return $"{GetColumnName(columnIndex)}{GetRowName(rowIndex)}";
        }

        public (int columnIndex, int rowIndex) ParseCellName(string cellName)
        {
            if (string.IsNullOrEmpty(cellName))
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.CellNameCannotBeEmpty
                    )
                );
            }

            var columnPart = string.Concat(cellName.TakeWhile(char.IsLetter));
            var rowPart = string.Concat(cellName.SkipWhile(char.IsLetter));

            if (string.IsNullOrEmpty(columnPart) || string.IsNullOrEmpty(rowPart))
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.InvalidCellNameFormat,
                        ("CellName", cellName)
                    )
                );
            }

            int columnIndex = GetColumnIndex(columnPart);
            int rowIndex = GetRowIndex(rowPart);

            return (columnIndex, rowIndex);
        }

        public bool IsValidCellName(string cellName)
        {
            if (string.IsNullOrEmpty(cellName))
                return false;

            try
            {
                ParseCellName(cellName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public string GetColumnName(int columnIndex)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.ColumnIndexMustBeNonNegative
                    )
                );
            }

            string columnName = "";
            int index = columnIndex;

            while (index >= 0)
            {
                columnName = (char)('A' + (index % 26)) + columnName;
                index = (index / 26) - 1;
            }

            return columnName;
        }

        public int GetColumnIndex(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.ColumnNameCannotBeEmpty
                    )
                );
            }

            int result = 0;
            foreach (char c in columnName.ToUpper())
            {
                if (c < 'A' || c > 'Z')
                {
                    throw new ArgumentException(
                        DataProcessor.FormatResource(
                            AppResources.InvalidCharacterInColumnName,
                            ("Character", c)
                        )
                    );
                }
                
                result = result * 26 + (c - 'A' + 1);
            }
            
            return result - 1;
        }

        public bool IsValidColumnName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return false;

            return columnName.All(c => char.IsLetter(c) && (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z'));
        }
        
        public string GetRowName(int rowIndex)
        {
            if (rowIndex < 0)
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.RowIndexMustBeNonNegative
                    )
                );
            }

            return (rowIndex + 1).ToString();
        }

        public int GetRowIndex(string rowName)
        {
            if (string.IsNullOrEmpty(rowName))
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.RowNameCannotBeEmpty
                    )
                );
            }

            if (int.TryParse(rowName, out int rowNumber) && rowNumber >= 1)
            {
                return rowNumber - 1;
            }

            throw new ArgumentException(
                DataProcessor.FormatResource(
                    AppResources.InvalidRowName,
                    ("RowName", rowName)
                )
            );
        }

        public bool IsValidRowName(string rowName)
        {
            if (string.IsNullOrEmpty(rowName))
                return false;

            return int.TryParse(rowName, out int result) && result >= 1;
        }
        
        public string GetCellRange(string startCell, string endCell)
        {
            var (startCol, startRow) = ParseCellName(startCell);
            var (endCol, endRow) = ParseCellName(endCell);

            return $"{startCell}:{endCell}";
        }

        public string[] ParseCellRange(string range)
        {
            if (string.IsNullOrEmpty(range))
                return Array.Empty<string>();

            var parts = range.Split(':');
            if (parts.Length != 2)
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.RangeMustBeInFormat
                    )
                );
            }

            string startCell = parts[0].Trim();
            string endCell = parts[1].Trim();

            if (!IsValidCellName(startCell) || !IsValidCellName(endCell))
            {
                throw new ArgumentException(
                    DataProcessor.FormatResource(
                        AppResources.InvalidCellNamesInRange
                    )
                );
            }

            var (startCol, startRow) = ParseCellName(startCell);
            var (endCol, endRow) = ParseCellName(endCell);

            var cells = new List<string>();
            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    cells.Add(GetCellName(col, row));
                }
            }

            return cells.ToArray();
        }
    }
}