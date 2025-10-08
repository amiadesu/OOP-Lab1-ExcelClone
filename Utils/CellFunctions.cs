using System;
using System.Linq;
using ExcelClone.Resources.Localization;
using ExcelClone.Values;

namespace ExcelClone.Utils;

public static class CellFunctions
{
    public static CellValue PowerOperator(CellValue left, CellValue right)
    {
        return new CellValue(Math.Pow(left.NumberValue, right.NumberValue));
    }

    public static CellValue MultiplicationOperator(CellValue left, CellValue right)
    {
        return left * right;
    }

    public static CellValue DivisionOperator(CellValue left, CellValue right)
    {
        if (right == 0)
        {
            throw new DivideByZeroException(DataProcessor.FormatResource(
                AppResources.CannotDivideBy0
            ));
        }
        return left / right;
    }

    public static CellValue AdditionOperator(CellValue left, CellValue right)
    {
        return left + right;
    }

    public static CellValue SubstractionOperator(CellValue left, CellValue right)
    {
        return left - right;
    }

    public static CellValue ModOperator(CellValue left, CellValue right)
    {
        if (right == 0)
        {
            throw new DivideByZeroException(DataProcessor.FormatResource(
                AppResources.CannotDivideBy0
            ));
        }
        return left % right;
    }

    public static CellValue DivOperator(CellValue left, CellValue right)
    {
        if (right == 0)
        {
            throw new DivideByZeroException(DataProcessor.FormatResource(
                AppResources.CannotDivideBy0
            ));
        }
        return new CellValue((int)(left / right));
    }

    public static CellValue LessThanOperator(CellValue left, CellValue right)
    {
        return new CellValue(left < right);
    }

    public static CellValue LessOrEqualOperator(CellValue left, CellValue right)
    {
        return new CellValue(left <= right);
    }

    public static CellValue GreaterThanOperator(CellValue left, CellValue right)
    {
        return new CellValue(left > right);
    }

    public static CellValue GreaterOrEqualOperator(CellValue left, CellValue right)
    {
        return new CellValue(left >= right);
    }

    public static CellValue EqualOperator(CellValue left, CellValue right)
    {
        return new CellValue(left == right);
    }

    public static CellValue NotEqualOperator(CellValue left, CellValue right)
    {
        return new CellValue(left != right);
    }

    public static CellValue AndOperator(CellValue left, CellValue right)
    {
        return left && right;
    }

    public static CellValue OrOperator(CellValue left, CellValue right)
    {
        return left || right;
    }

    /// <summary>
    /// Our spreadsheet does not work with text data, therefore
    /// we will consider left and right be logically equivalent iff
    /// they are both logical 0 or both not logical 0.
    /// </summary>
    public static CellValue LogicalEquivalentOperator(CellValue left, CellValue right)
    {
        return new CellValue(left.Equivalent(right));
    }



    public static CellValue SumFunction(CellValue[] args)
    {
        return CellValueLinq.Sum(args);
    }

    public static CellValue MinFunction(CellValue[] args)
    {
        if (args.Length != 2)
        {
            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.ExpectsExactlyNArguments,
                ("FunctionName", "MIN"),
                ("Count", 2)
            ));
        }
        return CellValueLinq.Min(args);
    }

    public static CellValue MaxFunction(CellValue[] args)
    {
        if (args.Length != 2)
        {
            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.ExpectsExactlyNArguments,
                ("FunctionName", "MAX"),
                ("Count", 2)
            ));
        }
        return CellValueLinq.Max(args);
    }

    public static CellValue MminFunction(CellValue[] args)
    {
        return CellValueLinq.Min(args);
    }

    public static CellValue MmaxFunction(CellValue[] args)
    {
        return CellValueLinq.Max(args);
    }

    public static CellValue IncrementFunction(CellValue[] args)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.ExpectsExactlyNArguments,
                ("FunctionName", "INC"),
                ("Count", 1)
            ));
        }
        return new CellValue(args[0] + 1);
    }

    public static CellValue DecrementFunction(CellValue[] args)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.ExpectsExactlyNArguments,
                ("FunctionName", "DEC"),
                ("Count", 1)
            ));
        }
        return new CellValue(args[0] - 1);
    }

    public static CellValue NotFunction(CellValue[] args)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException(DataProcessor.FormatResource(
                AppResources.ExpectsExactlyNArguments,
                ("FunctionName", "NOT"),
                ("Count", 1)
            ));
        }
        return new CellValue(args[0] == 0);
    }
}