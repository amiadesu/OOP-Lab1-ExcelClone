using ExcelClone.Components.CellStorage;
using ExcelClone.Services;

namespace Tests.Components;

public class SpreadsheetTests
{
    private readonly ICellNameService _cellNameService;
    private readonly ICellStorage _cellStorage;
    private const int _columns = 10;
    private const int _rows = 20;
    
    public SpreadsheetTests()
    {
        _cellNameService = new CellNameService();
        _cellStorage = new Spreadsheet(_cellNameService);
        _cellStorage.CreateNewCellStorage(_columns, _rows);
    }

    [Fact]
    public void InitializationTest()
    {
        Assert.Equal(_columns, _cellStorage.GetColumns());
        Assert.Equal(_rows, _cellStorage.GetRows());
    }

    [Fact]
    public void ExistingCellReferenceTest()
    {
        string cellReference = _cellNameService.GetCellName(0, 0);

        var result = _cellStorage.GetCellValue(cellReference);

        Assert.NotNull(result);
    }
}