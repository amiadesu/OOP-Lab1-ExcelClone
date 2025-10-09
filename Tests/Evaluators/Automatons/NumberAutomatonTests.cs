using ExcelClone.Evaluators.Automatons;

namespace Tests.Components;

public class NumberAutomatonTests
{
    private readonly IAutomaton _numberAutomaton;
    
    public NumberAutomatonTests()
    {
        _numberAutomaton = new NumberAutomaton();
    }

    [Theory]
    [InlineData(["", false])]
    [InlineData(["a", false])]
    [InlineData([".", true])]
    [InlineData([".1", true])]
    [InlineData(["-0.1", false])]
    [InlineData(["000000", true])]
    [InlineData(["000000..", false])]
    [InlineData(["000010000", true])]
    [InlineData(["000010000.000001", true])]
    public void StringTests(string s, bool expectedResult)
    {
        var result = _numberAutomaton.TestString(s);

        Assert.Equal(result, expectedResult);
    }

    [Theory]
    [InlineData(['.', true])]
    [InlineData(['0', true])]
    [InlineData(['-', false])]
    [InlineData(['+', false])]
    [InlineData(['9', true])]
    [InlineData(['a', false])]
    public void FirstCharTests(char c, bool expectedResult)
    {
        var result = _numberAutomaton.TestChar(c);

        Assert.Equal(result, expectedResult);
    }

    [Theory]
    [InlineData(["", false])]
    [InlineData(["a", false])]
    [InlineData([".", true])]
    [InlineData([".1", true])]
    [InlineData(["-0.1", false])]
    [InlineData(["000000", true])]
    [InlineData(["000000..", false])]
    [InlineData(["000010000", true])]
    [InlineData(["000010000.000001", true])]
    public void StringByCharsTests(string s, bool expectedResult)
    {
        foreach (var c in s)
        {
            _numberAutomaton.Insert(c);
        }

        var result = _numberAutomaton.IsAccepting();

        Assert.Equal(result, expectedResult);
    }
}