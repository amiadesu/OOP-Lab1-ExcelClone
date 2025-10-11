namespace ExcelClone.Components.CellStorage;

public interface ICellStorage : ICellStorageReader, ICellStorageWriter
{
    public void CreateNewCellStorage(int columns, int rows);
}