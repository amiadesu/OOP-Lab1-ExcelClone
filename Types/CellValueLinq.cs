using System;
using System.Linq;
using ExcelClone.Resources.Localization;
using ExcelClone.Utils;

namespace ExcelClone.Values;

public static class CellValueLinq
{
    public static CellValue Sum(params CellValue[] values)
    {
        double sum = values
            .Where(v => v.Type == CellValueType.Number)
            .Select(v => v.NumberValue)
            .Sum();

        return new CellValue(CellValueType.Number, numberValue: sum);
    }

    public static CellValue Min(params CellValue[] values)
    {
        var numericValues = values
            .Where(v => v.Type == CellValueType.Number)
            .Select(v => v.NumberValue)
            .ToArray();

        if (!numericValues.Any())
        {
            throw new InvalidOperationException(DataProcessor.FormatResource(
                AppResources.ExpectsAtLeastNNumberArguments,
                ("FunctionName", "MIN"),
                ("Count", 1)
            ));
        }

        double min = numericValues.Min();
        return new CellValue(CellValueType.Number, numberValue: min);
    }

    public static CellValue Max(params CellValue[] values)
    {
        var numericValues = values
            .Where(v => v.Type == CellValueType.Number)
            .Select(v => v.NumberValue)
            .ToArray();

        if (!numericValues.Any())
        {
            throw new InvalidOperationException(DataProcessor.FormatResource(
                AppResources.ExpectsAtLeastNNumberArguments,
                ("FunctionName", "MAX"),
                ("Count", 1)
            ));
        }

        double max = numericValues.Max();
        return new CellValue(CellValueType.Number, numberValue: max);
    }
}
