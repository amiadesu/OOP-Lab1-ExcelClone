using System;
using System.Linq;

namespace ExcelClone.Values
{
    public static class CellValueLinq
    {
        public static CellValue Sum(params CellValue[] values)
        {
            double sum = values
                .Where(v => v.Type == CellValueType.Number)
                .Select(v => v.NumberValue)  // Project to double
                .Sum();

            return new CellValue(CellValueType.Number, numberValue: sum);
        }

        public static CellValue Min(params CellValue[] values)
        {
            var numericValues = values
                .Where(v => v.Type == CellValueType.Number)
                .Select(v => v.NumberValue)  // Project to double
                .ToArray();

            if (!numericValues.Any())
                throw new InvalidOperationException("Min requires at least one numeric value");

            double min = numericValues.Min();
            return new CellValue(CellValueType.Number, numberValue: min);
        }

        public static CellValue Max(params CellValue[] values)
        {
            var numericValues = values
                .Where(v => v.Type == CellValueType.Number)
                .Select(v => v.NumberValue)  // Project to double
                .ToArray();

            if (!numericValues.Any())
                throw new InvalidOperationException("Max requires at least one numeric value");

            double max = numericValues.Max();
            return new CellValue(CellValueType.Number, numberValue: max);
        }
    }
}
