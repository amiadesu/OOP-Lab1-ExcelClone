using ExcelClone.Evaluators.Automatons;

namespace Tests.Components;

public class FunctionNameAutomatonTests
{
    private readonly IAutomaton _functionNameAutomaton;
    
    public FunctionNameAutomatonTests()
    {
        _functionNameAutomaton = new FunctionNameAutomaton();
    }

    [Theory]
    [InlineData(["", false])]
    [InlineData(["a", true])]
    [InlineData(["A", true])]
    [InlineData(["A1", true])]
    [InlineData(["A0", true])]
    [InlineData(["A-1", false])]
    [InlineData(["AA12", true])]
    [InlineData(["aa12", true])]
    [InlineData(["AA01", true])]
    [InlineData(["TEST2709", true])]
    public void StringTests(string s, bool expectedResult)
    {
        var result = _functionNameAutomaton.TestString(s);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(['A', true])]
    [InlineData(['Z', true])]
    [InlineData(['0', false])]
    [InlineData(['1', false])]
    [InlineData(['a', true])]
    public void FirstCharTests(char c, bool expectedResult)
    {
        var result = _functionNameAutomaton.TestChar(c);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(["", false])]
    [InlineData(["a", true])]
    [InlineData(["A", true])]
    [InlineData(["A1", true])]
    [InlineData(["A0", true])]
    [InlineData(["A-1", false])]
    [InlineData(["AA12", true])]
    [InlineData(["aa12", true])]
    [InlineData(["AA01", true])]
    [InlineData(["TEST2709", true])]
    public void StringByCharsTests(string s, bool expectedResult)
    {
        foreach (var c in s)
        {
            _functionNameAutomaton.Insert(c);
        }

        var result = _functionNameAutomaton.IsAccepting();

        Assert.Equal(expectedResult, result);
    }
}