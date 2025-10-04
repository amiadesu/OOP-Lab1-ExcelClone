namespace ExcelClone.Constants;

public static class Literals
{
    public const string errorMessage = "#ERROR";
    public const string refErrorMessage = "#REF";
    public static string prefix = "=";
    public static int prefixLength = prefix.Length;

    // Mask with 12 digits after dot
    public const string doubleFormatMask = "0.############";
    // 1e-13 because we store 12 digits after the dot in our spreadsheet
    public const double comparisonTolerance = 1e-13;
}