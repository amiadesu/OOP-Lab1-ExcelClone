using ExcelClone.Evaluators.Automatons;

namespace Tests.Components;

public class CellNameAutomatonTests
{
    private readonly IAutomaton _cellNameAutomaton;
    
    public CellNameAutomatonTests()
    {
        _cellNameAutomaton = new CellNameAutomaton();
    }

    [Theory]
    [InlineData(["", false])]
    [InlineData(["a", false])]
    [InlineData(["A", false])]
    [InlineData(["A1", true])]
    [InlineData(["A0", false])]
    [InlineData(["A-1", false])]
    [InlineData(["AA12", true])]
    [InlineData(["aa12", false])]
    [InlineData(["AA01", false])]
    [InlineData(["TEST2709", true])]
    public void StringTests(string s, bool expectedResult)
    {
        var result = _cellNameAutomaton.TestString(s);

        Assert.Equal(result, expectedResult);
    }

    [Theory]
    [InlineData(['A', true])]
    [InlineData(['Z', true])]
    [InlineData(['0', false])]
    [InlineData(['1', false])]
    [InlineData(['a', false])]
    public void FirstCharTests(char c, bool expectedResult)
    {
        var result = _cellNameAutomaton.TestChar(c);

        Assert.Equal(result, expectedResult);
    }

    [Theory]
    [InlineData(["", false])]
    [InlineData(["a", false])]
    [InlineData(["A", false])]
    [InlineData(["A1", true])]
    [InlineData(["A0", false])]
    [InlineData(["A-1", false])]
    [InlineData(["AA12", true])]
    [InlineData(["aa12", false])]
    [InlineData(["AA01", false])]
    [InlineData(["TEST2709", true])]
    public void StringByCharsTests(string s, bool expectedResult)
    {
        foreach (var c in s)
        {
            _cellNameAutomaton.Insert(c);
        }

        var result = _cellNameAutomaton.IsAccepting();

        Assert.Equal(result, expectedResult);
    }
}