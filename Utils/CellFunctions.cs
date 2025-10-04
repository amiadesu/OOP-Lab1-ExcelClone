using System;
using System.Linq;

namespace ExcelClone.Utils;

public static class CellFunctions
{
    public static double PowerOperator(double left, double right)
    {
        return Math.Pow(left, right);
    }

    public static double MultiplicationOperator(double left, double right)
    {
        return left * right;
    }

    public static double DivisionOperator(double left, double right)
    {
        if (DoubleChecker.Equal(right, 0))
        {
            throw new Exception("Cannot divide by 0");
        }
        return left / right;
    }

    public static double AdditionOperator(double left, double right)
    {
        return left + right;
    }

    public static double SubstractionOperator(double left, double right)
    {
        return left - right;
    }

    public static double ModOperator(double left, double right)
    {
        if (DoubleChecker.Equal(right, 0))
        {
            throw new Exception("Cannot divide by 0");
        }
        return left % right;
    }

    public static double DivOperator(double left, double right)
    {
        if (DoubleChecker.Equal(right, 0))
        {
            throw new Exception("Cannot divide by 0");
        }
        return (int)(left / right);
    }

    public static double LessThanOperator(double left, double right)
    {
        return left < right ? 1 : 0;
    }

    public static double LessOrEqualOperator(double left, double right)
    {
        return left <= right ? 1 : 0;
    }

    public static double GreaterThanOperator(double left, double right)
    {
        return left > right ? 1 : 0;
    }

    public static double GreaterOrEqualOperator(double left, double right)
    {
        return left >= right ? 1 : 0;
    }

    public static double EqualOperator(double left, double right)
    {
        return DoubleChecker.Equal(left, right) ? 1 : 0;
    }

    public static double NotEqualOperator(double left, double right)
    {
        return DoubleChecker.Equal(left, right) ? 0 : 1;
    }

    public static double AndOperator(double left, double right)
    {
        return DoubleChecker.Equal(left, 1) && DoubleChecker.Equal(right, 1) ? 1 : 0;
    }

    public static double OrOperator(double left, double right)
    {
        return DoubleChecker.Equal(left, 1) || DoubleChecker.Equal(right, 1) ? 1 : 0;
    }

    /// <summary>
    /// Our spreadsheet does not work with text data, therefore
    /// we will consider left and right be logically equivalent iff
    /// they are both logical 0 or both not logical 0.
    /// </summary>
    public static double LogicalEquivalentOperator(double left, double right)
    {
        return DoubleChecker.Equal(left, 0) == DoubleChecker.Equal(right, 0) ? 1 : 0;
    }



    public static double SumFunction(double[] args)
    {
        return args.Sum();
    }

    public static double MinFunction(double[] args)
    {
        if (args.Length != 2)
        {
            throw new Exception("MIN expects exactly 2 arguments");
        }
        return args.Min();
    }

    public static double MaxFunction(double[] args)
    {
        if (args.Length != 2)
        {
            throw new Exception("MAX expects exactly 2 arguments");
        }
        return args.Max();
    }

    public static double MminFunction(double[] args)
    {
        return args.Min();
    }

    public static double MmaxFunction(double[] args)
    {
        return args.Max();
    }

    public static double IncrementFunction(double[] args)
    {
        if (args.Length != 1)
        {
            throw new Exception("INC expects exactly 1 argument");
        }
        return args[0] + 1;
    }

    public static double DecrementFunction(double[] args)
    {
        if (args.Length != 1)
        {
            throw new Exception("DEC expects exactly 1 argument");
        }
        return args[0] - 1;
    }

    public static double NotFunction(double[] args)
    {
        if (args.Length != 1)
        {
            throw new Exception("NOT expects exactly 1 argument");
        }
        return DoubleChecker.Equal(args[0], 0) ? 1 : 0;
    }
}