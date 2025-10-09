using ExcelClone.Evaluators.Tokens;

namespace Tests.Components;

public class FormulaTokenizerTests
{
    private readonly IFormulaTokenizer _formulaTokenizer;
    
    public FormulaTokenizerTests()
    {
        _formulaTokenizer = new FormulaTokenizer();
    }

    [Theory]
    [InlineData(["", 0])]
    [InlineData(["123", 1])]
    [InlineData(["1+1", 3])]
    [InlineData(["=", 1])]
    [InlineData(["=1", 2])]
    [InlineData(["A0", 2])] // Should be firstly picked by CellNameAutomaton, not FormulaNameAutomaton, that will reject it
    [InlineData(["=A0", 3])]
    [InlineData(["=AA12+2", 4])]
    [InlineData(["min(aa12+2,3)", 8])]
    [InlineData(["=min(aa12+2, 3    )", 9])]
    public void TokenCountTests(string s, int expectedTokensCount)
    {
        var result = _formulaTokenizer.Tokenize(s).Count;

        Assert.Equal(expectedTokensCount, result);
    }

    [Fact]
    public void TokensSemanticsTest()
    {
        string test = "=min(aa12+2^AB12, 3    )";
        var expectedTokens = new[]
        {
            CreateToken(TokenType.Operator, "="), // In production code will be skipped/removed before passing to tokenizer
            CreateToken(TokenType.Function, "MIN"), // All the text should be capitalized
            CreateToken(TokenType.Parenthesis, "("),
            CreateToken(TokenType.Function, "AA12"),
            CreateToken(TokenType.Operator, "+"),
            CreateToken(TokenType.Number, "2"),
            CreateToken(TokenType.Operator, "^"),
            CreateToken(TokenType.CellReference, "AB12"),
            CreateToken(TokenType.Comma, ","),
            CreateToken(TokenType.Number, "3"),
            CreateToken(TokenType.Parenthesis, ")"),
        };

        var tokens = _formulaTokenizer.Tokenize(test);

        Assert.Equal(tokens.Count, expectedTokens.Count());

        for (int i = 0; i < tokens.Count; i++)
        {
            Assert.Equal(expectedTokens[i].Type, tokens[i].Type);
            Assert.Equal(expectedTokens[i].Value, tokens[i].Value);
        }
    }

    private Token CreateToken(TokenType type, string value)
    {
        Token token = new Token
        {
            Type = type,
            Value = value
        };

        return token;
    }
}