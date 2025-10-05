using System;
using System.Linq;
using ExcelClone.Values;

namespace ExcelClone.Utils;

public static class CellFunctions
{
    public static CellValue PowerOperator(CellValue left, CellValue right)
    {
        return Math.Pow(left.NumberValue, right.NumberValue);
    }

    public static CellValue MultiplicationOperator(CellValue left, CellValue right)
    {
        return left * right;
    }

    public static CellValue DivisionOperator(CellValue left, CellValue right)
    {
        if (right == 0)
        {
            throw new Exception("Cannot divide by 0");
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
            throw new Exception("Cannot divide by 0");
        }
        return left % right;
    }

    public static CellValue DivOperator(CellValue left, CellValue right)
    {
        if (right == 0)
        {
            throw new Exception("Cannot divide by 0");
        }
        return (int)(left / right);
    }

    public static CellValue LessThanOperator(CellValue left, CellValue right)
    {
        return left < right;
    }

    public static CellValue LessOrEqualOperator(CellValue left, CellValue right)
    {
        return left <= right;
    }

    public static CellValue GreaterThanOperator(CellValue left, CellValue right)
    {
        return left > right;
    }

    public static CellValue GreaterOrEqualOperator(CellValue left, CellValue right)
    {
        return left >= right;
    }

    public static CellValue EqualOperator(CellValue left, CellValue right)
    {
        return left == right;
    }

    public static CellValue NotEqualOperator(CellValue left, CellValue right)
    {
        return left != right;
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
        return left.Equivalent(right);
    }



    public static CellValue SumFunction(CellValue[] args)
    {
        return CellValueLinq.Sum(args);
    }

    public static CellValue MinFunction(CellValue[] args)
    {
        if (args.Length != 2)
        {
            throw new Exception("MIN expects exactly 2 arguments");
        }
        return CellValueLinq.Min(args);
    }

    public static CellValue MaxFunction(CellValue[] args)
    {
        if (args.Length != 2)
        {
            throw new Exception("MAX expects exactly 2 arguments");
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
            throw new Exception("INC expects exactly 1 argument");
        }
        return args[0] + 1;
    }

    public static CellValue DecrementFunction(CellValue[] args)
    {
        if (args.Length != 1)
        {
            throw new Exception("DEC expects exactly 1 argument");
        }
        return args[0] - 1;
    }

    public static CellValue NotFunction(CellValue[] args)
    {
        if (args.Length != 1)
        {
            throw new Exception("NOT expects exactly 1 argument");
        }
        return args[0] == 0;
    }
}