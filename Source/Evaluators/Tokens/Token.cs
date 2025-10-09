using ExcelClone.Values;

namespace ExcelClone.Evaluators.Tokens;

public enum TokenType
{
    Number,
    Text,
    CellReference,
    CellValue,
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
    public CellValue? CellValue = null;
    public int Position { get; set; }
}