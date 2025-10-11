using System.IO;
using ExcelClone.Components.CellStorage;
using ExcelClone.Services;

namespace ExcelClone.Helpers;

public interface ITableDataFormatter
{
    public string ToTableFormat(ICellStorageReader spreadsheet);
    public Spreadsheet FromTableFormat(StreamReader reader, ICellNameService nameService);
}