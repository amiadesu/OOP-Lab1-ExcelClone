using System.Globalization;
using ExcelClone.Constants;

namespace ExcelClone.Utils;

public static class DataFormatter
{
    public static string FormatFloatingPoint(double number)
    {
        // If it's an integer, return as integer
        if (DoubleChecker.Equal(number % 1, 0))
        {
            return ((long)number).ToString(CultureInfo.InvariantCulture);
        }

        // Always show leading zero for fractions like 0.123
        // Trim trailing zeros but leave at least one digit after decimal
        string s = number.ToString(Literals.doubleFormatMask, CultureInfo.InvariantCulture);

        return s;
    }
}