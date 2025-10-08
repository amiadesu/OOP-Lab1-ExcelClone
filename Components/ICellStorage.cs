using ExcelClone.Values;

namespace ExcelClone.Components;

public interface ICellStorage : ICellStorageReader, ICellStorageWriter
{
    public void CreateNewCellStorage(int columns, int rows);
}