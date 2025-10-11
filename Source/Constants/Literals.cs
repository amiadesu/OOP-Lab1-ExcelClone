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

    public const string prefix = "=";
    public static readonly int prefixLength = prefix.Length;

    // Mask with 12 digits after dot
    public const string doubleFormatMask = "0.############";
    // 1e-13 because we store 12 digits after the dot in our spreadsheet
    public const double comparisonTolerance = 1e-13;

    public const long authBufferSeconds = 10;
    public const string authURL = "https://accounts.google.com/o/oauth2/v2/auth";
    public const int authFlowPort = 42135;
}