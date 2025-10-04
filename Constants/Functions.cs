using System;
using System.Collections.Generic;
using ExcelClone.Utils;

namespace ExcelClone.Constants;

public enum Associativity
{
    Left,
    Right
};

public static class Functions
{
    /// <summary>
    /// Basic operators with infix notation.<br/>
    /// https://en.wikipedia.org/wiki/Infix_notation<br/>
    /// This type of notation only support 2 arguments (left and right).<br/>
    /// Can have any type of associativity.
    /// </summary>
    public static readonly string basicOperators = "+-*/^<=>";
    /// <summary>
    /// https://en.wikipedia.org/wiki/Infix_notation<br/>
    /// This type of notation only support 2 arguments (left and right).<br/>
    /// We will treat functions from here as left-associative.
    /// </summary>
    public static readonly string[] infixNotationFunctions = [
        "MOD", "DIV", "OR", "AND", "EQV"
    ];
    /// <summary>
    /// https://en.wikipedia.org/wiki/Polish_notation (also called prefix notation)<br/>
    /// For these functions associavity does not matter.
    /// </summary>
    public static readonly string[] prefixNotationFunctions = [
        "MIN", "MAX", "MMIN", "MMAX", "INC", "DEC", "NOT"
    ];

    public static readonly Dictionary<string, (int precedence, Associativity associativity)> infixFunctionsInfo =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "^", (16, Associativity.Right) },
            { "*", (15, Associativity.Left) },
            { "/", (15, Associativity.Left) },
            { "+", (14, Associativity.Left) },
            { "-", (14, Associativity.Left) },

            { "MOD", (14, Associativity.Left) },
            { "DIV", (14, Associativity.Left) },

            { "<", (13, Associativity.Left) },
            { ">", (13, Associativity.Left) },
            { "<=", (13, Associativity.Left) },
            { ">=", (13, Associativity.Left) },
            { "=", (13, Associativity.Left) },
            { "<>", (12, Associativity.Left) },

            { "AND", (11, Associativity.Left) },
            { "OR", (10, Associativity.Left) },
            { "EQV", (9, Associativity.Left) },
        };

    public static readonly Dictionary<string, Func<double, double, double>> operatorFunctions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["^"] = CellFunctions.PowerOperator,
            ["*"] = CellFunctions.MultiplicationOperator,
            ["/"] = CellFunctions.DivisionOperator,
            ["+"] = CellFunctions.AdditionOperator,
            ["-"] = CellFunctions.SubstractionOperator,

            ["MOD"] = CellFunctions.ModOperator,
            ["DIV"] = CellFunctions.DivOperator,

            ["<"] = CellFunctions.LessThanOperator,
            ["<="] = CellFunctions.LessOrEqualOperator,
            [">"] = CellFunctions.GreaterThanOperator,
            [">="] = CellFunctions.GreaterOrEqualOperator,
            ["="] = CellFunctions.EqualOperator,
            ["<>"] = CellFunctions.NotEqualOperator,

            ["AND"] = CellFunctions.AndOperator,
            ["OR"] = CellFunctions.OrOperator,
            ["EQV"] = CellFunctions.LogicalEquivalentOperator,
        };

    public static readonly Dictionary<string, Func<double[], double>> prefixFunctions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SUM"]  = CellFunctions.SumFunction,
            ["MIN"]  = CellFunctions.MinFunction,
            ["MAX"]  = CellFunctions.MaxFunction,
            ["MMIN"] = CellFunctions.MminFunction,
            ["MMAX"] = CellFunctions.MmaxFunction,
            ["INC"]  = CellFunctions.IncrementFunction,
            ["DEC"]  = CellFunctions.DecrementFunction,
            ["NOT"]  = CellFunctions.NotFunction,
        };
}