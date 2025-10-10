using ExcelClone.Constants;
using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Parsers;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Services;
using ExcelClone.Values;

namespace Tests.Components;

public class FormulaParserServiceTests
{
    private readonly IFormulaTokenizer _formulaTokenizer;
    private readonly IParser _parser;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IFormulaParserService _formulaParserService;
    
    public FormulaParserServiceTests()
    {
        _formulaTokenizer = new FormulaTokenizer();
        _parser = new Parser(null);
        _formulaEvaluator = new FormulaEvaluator(_parser);
        _formulaParserService = new FormulaParserService(_formulaTokenizer, _formulaEvaluator);
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
        var result = _formulaParserService.Evaluate(formula);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(expectedResult, result.Result.Value);
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
        var result = _formulaParserService.Evaluate(formula);

        Assert.NotNull(result.ErrorMessage);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public void ReferenceWithNoStorageTest()
    {
        string formula = "=A1";

        var result = _formulaParserService.Evaluate(formula);

        Assert.Equal(CellValueType.RefError, result.Result.Type);
    }
}