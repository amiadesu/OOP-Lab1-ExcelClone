using System;

namespace ExcelClone.Exceptions;

public class FormulaParseException : Exception
{
    public FormulaParseException(string message) : base(message) { }
}