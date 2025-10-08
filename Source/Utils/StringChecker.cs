using ExcelClone.Constants;
using ExcelClone.Evaluators.Automatons;

namespace ExcelClone.Utils;

public static class StringChecker
{
    public static bool IsError(string s)
    {
        return s == Literals.errorMessage || s == Literals.refErrorMessage;
    }

    public static bool IsSignedNumber(string s)
    {
        NumberAutomaton numberAutomaton = new NumberAutomaton();
        string temp = s;
        if (temp.StartsWith('-') || temp.StartsWith('+'))
        {
            temp = temp[1..];
        }
        return numberAutomaton.TestString(temp);
    }
}