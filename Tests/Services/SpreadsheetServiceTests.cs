using ExcelClone.Components.CellStorage;
using ExcelClone.Components.Trees;
using ExcelClone.Constants;
using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Parsers;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Services;

namespace Tests.Components;

public class SpreadsheetServiceTests
{
    private readonly ICellNameService _cellNameService;
    private readonly IDependencyTree _dependencyTree;
    private readonly ICellStorage _cellStorage;
    private readonly IFormulaTokenizer _formulaTokenizer;
    private readonly IParser _parser;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IFormulaParserService _formulaParserService;
    private readonly ISpreadsheetService _spreadsheetService;
    private const int _columns = 10;
    private const int _rows = 20;
    
    public SpreadsheetServiceTests()
    {
        _cellNameService = new CellNameService();
        _dependencyTree = new DependencyTree();

        _cellStorage = new Spreadsheet(_cellNameService);
        _cellStorage.CreateNewCellStorage(_columns, _rows);

        _formulaTokenizer = new FormulaTokenizer();
        _parser = new Parser(_cellStorage);
        _formulaEvaluator = new FormulaEvaluator(_parser);
        _formulaParserService = new FormulaParserService(_formulaTokenizer, _formulaEvaluator);

        _spreadsheetService = new SpreadsheetService(_cellStorage, _dependencyTree, _formulaParserService);
    }

    [Theory]
    [InlineData(["", ""])]
    [InlineData(["123", "123"])]
    [InlineData(["1+1", "1+1"])]
    [InlineData(["1++1", "1++1"])]
    [InlineData(["1+-1", "1+-1"])]
    [InlineData(["min(12+2,3)", "min(12+2,3)"])]
    [InlineData(["=123", "123"])]
    [InlineData(["=1+1", "2"])]
    [InlineData(["=1++1", "2"])]
    [InlineData(["=1+-1", "0"])]
    [InlineData(["=min(12+2,3)", "3"])]
    [InlineData(["=1=1", Literals.trueLiteral])]
    [InlineData(["=1<>1", Literals.falseLiteral])]
    [InlineData(["=mmin(12+2, 3    ,max(2^.0,4 mod 2))", "1"])]
    public void CorrectFormulaEvaluationTests(string formula, string expectedResult)
    {
        var errorMessage = _spreadsheetService.UpdateCellFormula("A1", formula);
        var result = _cellStorage.GetCellValue("A1");

        Assert.Null(errorMessage);
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Value);
    }

    [Theory]
    [InlineData(["="])]
    [InlineData(["=2+"])]
    [InlineData(["=3^^3"])]
    [InlineData(["=(3 <> 2) + 1"])] // Logical type + number type is undefined
    [InlineData(["=min(1,2,3)"])]
    [InlineData(["=min(1,2)("])] // Formula parser is not OK with extra tokens
    [InlineData(["=1/0"])]
    [InlineData(["=A"])]
    public void IncorrectFormulaEvaluationTests(string formula)
    {
        var errorMessage = _spreadsheetService.UpdateCellFormula("A1", formula);

        Assert.NotNull(errorMessage);
        Assert.NotEmpty(errorMessage);
    }

    [Fact]
    public void FormulaUpdateWithDependenciesTest()
    {
        var updatesFirst = new[]
        {
            ("A1", "1"),
            ("A2", "=2"),
            ("A3", "=A1+A2*2"),
            ("A4", "=A3=5")
        };

        foreach (var update in updatesFirst)
        {
            var errorMessageFirst = _spreadsheetService.UpdateCellFormula(update.Item1, update.Item2);

            Assert.Null(errorMessageFirst);
        }

        var result1 = _cellStorage.GetCellValue("A4");

        Assert.NotNull(result1);
        Assert.Equal(Literals.trueLiteral, result1.Value);

        var updatesSecond = new[]
        {
            ("A1", "2"),
            ("A2", "=1")
        };

        foreach (var update in updatesSecond)
        {
            var errorMessageSecond = _spreadsheetService.UpdateCellFormula(update.Item1, update.Item2);

            Assert.Null(errorMessageSecond);
        }

        var result2 = _cellStorage.GetCellValue("A4");

        Assert.NotNull(result2);
        Assert.Equal(Literals.falseLiteral, result2.Value);

        var errorMessageThird = _spreadsheetService.UpdateCellFormula("A1", "=A3");

        Assert.NotNull(errorMessageThird);
        Assert.NotEmpty(errorMessageThird);
    }
}