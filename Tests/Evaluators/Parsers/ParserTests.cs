using ExcelClone.Evaluators.Parsers;
using ExcelClone.Evaluators.Tokens;

namespace Tests.Components;

public class ParserTests
{
    private readonly IFormulaTokenizer _formulaTokenizer;
    private readonly IParser _parser;
    
    public ParserTests()
    {
        _formulaTokenizer = new FormulaTokenizer();
        _parser = new Parser(null);
    }

    [Theory]
    [InlineData(["123", 123])]
    [InlineData(["1+1", 2])]
    [InlineData(["1++1", 2])]
    [InlineData(["1+-1", 0])]
    [InlineData(["min(12+2,3)", 3])]
    [InlineData(["mmin(12+2, 3    ,max(2^.0,4 mod 2))", 1])]
    public void CorrectExpressionParsingTests(string s, int expectedResult)
    {
        var tokens = _formulaTokenizer.Tokenize(s);
        var result = _parser.ParseExpression(tokens);

        Assert.Equal(expectedResult, result.NumberValue);
    }

    [Theory]
    [InlineData([""])]
    [InlineData(["="])]
    [InlineData(["2+"])]
    [InlineData(["3^^3"])]
    [InlineData(["(3 <> 2) + 1"])] // Logical type + number type is undefined
    [InlineData(["min(1,2,3)"])]
    [InlineData(["1/0"])]
    // Parser is OK with extra tokens
    public void IncorrectExpressionParsingTests(string s)
    {
        var tokens = _formulaTokenizer.Tokenize(s);

        Assert.ThrowsAny<Exception>(() => _parser.ParseExpression(tokens));
    }
}