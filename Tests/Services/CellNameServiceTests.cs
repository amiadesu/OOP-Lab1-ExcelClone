using ExcelClone.Services;

namespace Tests.Components;

public class CellNameServiceTests
{
    private readonly ICellNameService _cellNameService;
    
    public CellNameServiceTests()
    {
        _cellNameService = new CellNameService();
    }

    [Theory]
    [InlineData([0, 0, "A1"])]
    [InlineData([3, 99, "D100"])]
    [InlineData([25, 989, "Z990"])]
    [InlineData([26, 13, "AA14"])]
    [InlineData([703, 133, "AAB134"])]
    public void GetCellNameTests(int col, int row, string expectedResult)
    {
        var result = _cellNameService.GetCellName(col, row);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(["A1", 0, 0])]
    [InlineData(["D100", 3, 99])]
    [InlineData(["Z990", 25, 989])]
    [InlineData(["AA14", 26, 13])]
    [InlineData(["AAB134", 703, 133])]
    public void ParseCellNameTests(string cellName, int expectedCol, int expectedRow)
    {
        var result = _cellNameService.ParseCellName(cellName);

        Assert.Equal(expectedCol, result.columnIndex);
        Assert.Equal(expectedRow, result.rowIndex);
    }
}