using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExcelClone.Services
{
    public class CellNameService : ICellNameService
    {
        // Default implementation - Excel-style (A1, B2, etc.)
        // But you can easily extend this to support different naming schemes
        
        #region Cell Naming
        
        public string GetCellName(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || rowIndex < 0)
                throw new ArgumentException("Column and row indices must be non-negative");
            
            return $"{GetColumnName(columnIndex)}{GetRowName(rowIndex)}";
        }

        public (int columnIndex, int rowIndex) ParseCellName(string cellName)
        {
            if (string.IsNullOrEmpty(cellName))
                throw new ArgumentException("Cell name cannot be null or empty");

            // Try to split into column and row parts
            var columnPart = string.Concat(cellName.TakeWhile(char.IsLetter));
            var rowPart = string.Concat(cellName.SkipWhile(char.IsLetter));

            if (string.IsNullOrEmpty(columnPart) || string.IsNullOrEmpty(rowPart))
                throw new ArgumentException($"Invalid cell name format: {cellName}");

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
        
        #endregion

        #region Column Naming - Excel-style (A, B, ..., Z, AA, AB, ...)
        
        public string GetColumnName(int columnIndex)
        {
            if (columnIndex < 0)
                throw new ArgumentException("Column index must be non-negative");

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
                throw new ArgumentException("Column name cannot be null or empty");

            int result = 0;
            foreach (char c in columnName.ToUpper())
            {
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException($"Invalid character in column name: {c}");
                
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
        
        #endregion

        #region Row Naming - Default to numbers (1, 2, 3, ...) but extensible
        
        public string GetRowName(int rowIndex)
        {
            if (rowIndex < 0)
                throw new ArgumentException("Row index must be non-negative");
            
            // Default: 1-based numbers
            return (rowIndex + 1).ToString();
            
            // Example alternatives (uncomment to use):
            // return GetRomanNumeral(rowIndex + 1); // I, II, III, IV, ...
            // return GetAlphaRowName(rowIndex);     // A, B, C, ..., AA, AB, ...
        }

        public int GetRowIndex(string rowName)
        {
            if (string.IsNullOrEmpty(rowName))
                throw new ArgumentException("Row name cannot be null or empty");

            // Default: parse as 1-based number
            if (int.TryParse(rowName, out int rowNumber) && rowNumber >= 1)
            {
                return rowNumber - 1;
            }
            
            // Example alternatives (uncomment to use):
            // return ParseRomanNumeral(rowName) - 1;
            // return ParseAlphaRowName(rowName);
            
            throw new ArgumentException($"Invalid row name: {rowName}");
        }

        public bool IsValidRowName(string rowName)
        {
            if (string.IsNullOrEmpty(rowName))
                return false;

            // Default: must be a positive integer
            return int.TryParse(rowName, out int result) && result >= 1;
            
            // For alternative row naming schemes, add validation here
        }
        
        #endregion

        #region Range Operations
        
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
                throw new ArgumentException("Range must be in format 'startCell:endCell'");

            string startCell = parts[0].Trim();
            string endCell = parts[1].Trim();

            if (!IsValidCellName(startCell) || !IsValidCellName(endCell))
                throw new ArgumentException("Invalid cell names in range");

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
        
        #endregion
    }
}