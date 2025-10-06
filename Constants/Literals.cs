namespace ExcelClone.Constants;

public static class Literals
{
    public const string errorMessage = "#ERROR";
    public const string refErrorMessage = "#REF";
    public const string trueLiteral = "TRUE";
    public const string falseLiteral = "FALSE";
    public const string defaultFileName = "result.table";
    public const string defaultHistoryFileName = "file_history.txt";

    public const int maxAmountOfRows = 100;
    public const int maxAmountOfColumns = 100;

    public const int cellWidth = 100;
    public const int cellHeight = 25;

    public static string prefix = "=";
    public static int prefixLength = prefix.Length;

    // Mask with 12 digits after dot
    public const string doubleFormatMask = "0.############";
    // 1e-13 because we store 12 digits after the dot in our spreadsheet
    public const double comparisonTolerance = 1e-13;
}