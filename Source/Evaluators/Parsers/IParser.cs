using System.Collections.Generic;
using ExcelClone.Evaluators.Tokens;
using ExcelClone.Values;

namespace ExcelClone.Evaluators.Parsers;

public interface IParser
{
    public CellValue ParseExpression(List<Token> tokens);
    public bool IsAtEnd();
    public Token? CurrentToken();
}