namespace ExcelClone.Evaluators.Tokens;

public enum TokenType
{
    Number,
    Text,
    CellReference,
    Function,
    Operator,
    Parenthesis,
    Comma,
    Colon
}

public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; } = "";
    public int Position { get; set; }
}