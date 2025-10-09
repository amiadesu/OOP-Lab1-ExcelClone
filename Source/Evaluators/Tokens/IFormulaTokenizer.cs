using System.Collections.Generic;

namespace ExcelClone.Evaluators.Tokens;

public interface IFormulaTokenizer
{
    public List<Token> Tokenize(string expression);
}