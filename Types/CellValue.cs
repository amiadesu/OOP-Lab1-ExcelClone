using System;
using ExcelClone.Constants;
using ExcelClone.Evaluators.Automatons;
using ExcelClone.Utils;

namespace ExcelClone.Values;

public enum CellValueType
{
    Text,
    Number,
    Boolean,
    RefError,
    GeneralError
}

public class CellValue : IComparable<CellValue>
{
    public CellValueType Type { get; set; }

    public double NumberValue { get; set; } = 0;
    private string _value = "";
    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            ProcessValue();
        }
    }

    public CellValue(CellValueType type, double numberValue = 0, string value = "")
    {
        Type = type;
        NumberValue = numberValue;
        if (Type != CellValueType.Text)
        {
            _value = this.ToString();
        }
        else
        {
            Value = value;
        }
    }

    public static implicit operator CellValue(double value)
    {
        return new CellValue(CellValueType.Number, numberValue: value);
    }

    public static implicit operator CellValue(string value)
    {
        return new CellValue(CellValueType.Text, value: value);
    }

    public static implicit operator CellValue(bool value)
    {
        return new CellValue(CellValueType.Boolean, numberValue: value ? 1 : 0);
    }

    // === Comparisons ===
    public static bool operator >(CellValue a, CellValue b) => a.CompareTo(b) > 0;
    public static bool operator <(CellValue a, CellValue b) => a.CompareTo(b) < 0;
    public static bool operator >=(CellValue a, CellValue b) => a.CompareTo(b) >= 0;
    public static bool operator <=(CellValue a, CellValue b) => a.CompareTo(b) <= 0;
    public static bool operator ==(CellValue? a, CellValue? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Type == b.Type && DoubleChecker.Equal(a.NumberValue, b.NumberValue) && a.Value == b.Value;
    }
    public static bool operator !=(CellValue a, CellValue b) => !(a == b);

    public static bool operator ==(CellValue a, double b)
    {
        return DoubleChecker.Equal(a.NumberValue, b);
    }
    public static bool operator !=(CellValue a, double b)
    {
        return !DoubleChecker.Equal(a.NumberValue, b);
    }

    // Bitwise AND → used by &&
    public static CellValue operator &(CellValue a, CellValue b)
    {
        return new CellValue(CellValueType.Boolean,
            numberValue: ((bool)a && (bool)b) ? 1 : 0);
    }

    // Bitwise OR → used by ||
    public static CellValue operator |(CellValue a, CellValue b)
    {
        return new CellValue(CellValueType.Boolean,
            numberValue: ((bool)a || (bool)b) ? 1 : 0);
    }

    // Required for short-circuiting with && and ||
    public static bool operator true(CellValue a) => (bool)a;
    public static bool operator false(CellValue a) => !(bool)a;

    // === Arithmetic operators (only valid for numbers) ===
    public static CellValue operator +(CellValue a, CellValue b)
    {
        if (a.Type == CellValueType.Number && b.Type == CellValueType.Number)
            return new CellValue(CellValueType.Number, numberValue: a.NumberValue + b.NumberValue);
        throw new InvalidOperationException("Operator + is only valid for numbers");
    }

    public static CellValue operator +(CellValue cell)
    {
        if (cell.Type == CellValueType.Number)
            return new CellValue(CellValueType.Number, numberValue: +cell.NumberValue);

        throw new InvalidOperationException("Unary plus is only valid for numbers");
    }

    public static CellValue operator -(CellValue a, CellValue b)
    {
        if (a.Type == CellValueType.Number && b.Type == CellValueType.Number)
            return new CellValue(CellValueType.Number, numberValue: a.NumberValue - b.NumberValue);
        throw new InvalidOperationException("Operator - is only valid for numbers");
    }

    public static CellValue operator -(CellValue cell)
    {
        if (cell.Type == CellValueType.Number)
            return new CellValue(CellValueType.Number, numberValue: -cell.NumberValue);

        throw new InvalidOperationException("Unary minus is only valid for numbers");
    }

    public static CellValue operator *(CellValue a, CellValue b)
    {
        if (a.Type == CellValueType.Number && b.Type == CellValueType.Number)
            return new CellValue(CellValueType.Number, numberValue: a.NumberValue * b.NumberValue);
        throw new InvalidOperationException("Operator * is only valid for numbers");
    }

    public static CellValue operator /(CellValue a, CellValue b)
    {
        if (a.Type == CellValueType.Number && b.Type == CellValueType.Number)
        {
            if (DoubleChecker.Equal(b.NumberValue, 0))
                throw new DivideByZeroException();
            return new CellValue(CellValueType.Number, numberValue: a.NumberValue / b.NumberValue);
        }
        throw new InvalidOperationException("Operator / is only valid for numbers");
    }

    public static CellValue operator %(CellValue a, CellValue b)
    {
        if (a.Type == CellValueType.Number && b.Type == CellValueType.Number)
            return new CellValue(CellValueType.Number, numberValue: a.NumberValue % b.NumberValue);
        throw new InvalidOperationException("Operator % is only valid for numbers");
    }

    // === Conversion operators ===
    public static implicit operator string(CellValue cell) => cell.ToString();

    public static explicit operator double(CellValue cell)
    {
        if (cell.Type == CellValueType.Number) return cell.NumberValue;
        throw new InvalidCastException($"Cannot convert {cell.Type} to double");
    }

    public static explicit operator bool(CellValue cell)
    {
        return cell.Type switch
        {
            CellValueType.Boolean => string.Equals(cell.Value, "TRUE", StringComparison.OrdinalIgnoreCase),
            CellValueType.Number => !DoubleChecker.Equal(cell.NumberValue, 0),
            CellValueType.Text => !string.IsNullOrEmpty(cell.Value),
            _ => throw new InvalidCastException($"Cannot convert {cell.Type} to bool")
        };
    }

    // === IComparable implementation ===
    public int CompareTo(CellValue? other)
    {
        if (other is null) return 1;

        // Compare numbers
        if (Type == CellValueType.Number && other.Type == CellValueType.Number)
        {
            if (DoubleChecker.Equal(NumberValue, other.NumberValue))
                return 0;
            return NumberValue < other.NumberValue ? -1 : 1;
        }

        if (Value != other.Value)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        return Type.CompareTo(other.Type);
    }

    // === Overrides ===
    public override string ToString()
    {
        return Type switch
        {
            CellValueType.Number => DataProcessor.FormatFloatingPoint(NumberValue),
            CellValueType.Boolean => DoubleChecker.Equal(NumberValue, 1) ? "TRUE" : "FALSE",
            CellValueType.RefError => Literals.refErrorMessage,
            CellValueType.GeneralError => Literals.errorMessage,
            _ => Value
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CellValue other) return false;
        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, NumberValue, Value);
    }

    public bool Equivalent(CellValue other)
    {
        return this.Value == other.Value || DoubleChecker.Equal(this.NumberValue, other.NumberValue);
    }

    public void ProcessValue()
    {
        if (Value == "FALSE")
        {
            NumberValue = 0;
            Type = CellValueType.Boolean;
        }
        else if (Value == "TRUE")
        {
            NumberValue = 1;
            Type = CellValueType.Boolean;
        }
        else if (Value == Literals.refErrorMessage)
        {
            NumberValue = 0;
            Type = CellValueType.RefError;
        }
        else if (Value == Literals.errorMessage)
        {
            NumberValue = 0;
            Type = CellValueType.GeneralError;
        }
        else
        {
            NumberAutomaton numberAutomaton = new NumberAutomaton();
            if (numberAutomaton.TestString(Value))
            {
                NumberValue = DataProcessor.StringToDouble(Value);
                Type = CellValueType.Number;
            }
        }
    }

    public void Clear()
    {
        Type = CellValueType.Text;
        NumberValue = 0;
        Value = "";
    }
}
