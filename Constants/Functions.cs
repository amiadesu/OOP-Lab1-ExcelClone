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

    public static readonly HashSet<string> rightAssociativeFunctions = new HashSet<string>
    {
        "^"
    };

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

    public static readonly Dictionary<string, Func<double, double, double>> operatorFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["^"] = (l, r) => Math.Pow(l, r),
        ["*"] = (l, r) => l * r,
        ["/"] = (l, r) => l / r,
        ["+"] = (l, r) => l + r,
        ["-"] = (l, r) => l - r,
        
        ["MOD"] = (l, r) => l % r,
        ["DIV"] = (l, r) => (int)(l / r),

        ["<"] = (l, r) => l < r ? 1 : 0,
        ["<="] = (l, r) => l <= r ? 1 : 0,
        [">"] = (l, r) => l > r ? 1 : 0,
        [">="] = (l, r) => l >= r ? 1 : 0,
        ["="] = (l, r) => DoubleChecker.Equal(l, r) ? 1 : 0,
        ["<>"] = (l, r) => DoubleChecker.Equal(l, r) ? 0 : 1,

        ["AND"] = (l, r) => DoubleChecker.Equal(l, 1) && DoubleChecker.Equal(r, 1) ? 1 : 0,
        ["OR"] = (l, r) => DoubleChecker.Equal(l, 1) || DoubleChecker.Equal(r, 1) ? 1 : 0,
        ["EQV"] = (l, r) => DoubleChecker.Equal(l, 0) == DoubleChecker.Equal(r, 0) ? 1 : 0,
    };
}