using ExcelClone.Evaluators;
using ExcelClone.Evaluators.Parsers;
using ExcelClone.Evaluators.Tokens;

namespace Tests.Components;

public class FormulaEvaluatorTests
{
    private readonly IFormulaTokenizer _formulaTokenizer;
    private readonly IParser _parser;
    private readonly IFormulaEvaluator _formulaEvaluator;
    
    public FormulaEvaluatorTests()
    {
        _formulaTokenizer = new FormulaTokenizer();
        _parser = new Parser(null);
        _formulaEvaluator = new FormulaEvaluator(_parser);
    }

    [Theory]
    [InlineData(["123", 123])]
    [InlineData(["1+1", 2])]
    [InlineData(["1++1", 2])]
    [InlineData(["1+-1", 0])]
    [InlineData(["min(12+2,3)", 3])]
    [InlineData(["mmin(12+2, 3    ,max(2^.0,4 mod 2))", 1])]
    public void CorrectExpressionEvaluationTests(string s, int expectedResult)
    {
        var tokens = _formulaTokenizer.Tokenize(s);
        var result = _formulaEvaluator.Evaluate(tokens);

        Assert.Equal(expectedResult, result.NumberValue);
    }

    [Theory]
    [InlineData([""])]
    [InlineData(["="])]
    [InlineData(["2+"])]
    [InlineData(["3^^3"])]
    [InlineData(["(3 <> 2) + 1"])] // Logical type + number type is undefined
    [InlineData(["min(1,2,3)"])]
    [InlineData(["min(1,2)("])] // Formula evaluator is not OK with extra tokens
    [InlineData(["1/0"])]
    public void IncorrectExpressionEvaluationTests(string s)
    {
        var tokens = _formulaTokenizer.Tokenize(s);

        Assert.ThrowsAny<Exception>(() => _formulaEvaluator.Evaluate(tokens));
    }
}