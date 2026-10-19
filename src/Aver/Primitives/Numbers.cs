namespace Aver;

internal static class Numbers
{
    public static bool ValueEquals(object? a, object? b)
        => object.Equals(a, b) || (a is not null && b is not null && Equal(a, b));

    public static bool Equal(object a, object b)
    {
        if (!TryRead(a, out var x) || !TryRead(b, out var y))
        {
            return false;
        }

        if (x.Kind is Kind.Decimal or Kind.Double || y.Kind is Kind.Decimal or Kind.Double)
        {
            if (x.Kind is Kind.Decimal or Kind.Double && y.Kind is Kind.Decimal or Kind.Double)
            {
                return FractionalEqual(x, y);
            }

            return x.Kind is Kind.Decimal or Kind.Double ? ExactEqual(x, y) : ExactEqual(y, x);
        }

        return x.Kind switch
        {
            Kind.Signed when y.Kind == Kind.Signed => x.Signed == y.Signed,
            Kind.Unsigned when y.Kind == Kind.Unsigned => x.Unsigned == y.Unsigned,
            Kind.Signed => x.Signed >= 0 && (ulong)x.Signed == y.Unsigned,
            _ => y.Signed >= 0 && (ulong)y.Signed == x.Unsigned,
        };
    }

    private static bool FractionalEqual(Number x, Number y)
    {
        if (x.Kind == Kind.Double && y.Kind == Kind.Double)
        {
            return x.Double == y.Double || (double.IsNaN(x.Double) && double.IsNaN(y.Double));
        }

        if (x.Kind == Kind.Decimal && y.Kind == Kind.Decimal)
        {
            return x.Decimal == y.Decimal;
        }

        var d = x.Kind == Kind.Decimal ? x.Decimal : y.Decimal;
        var f = x.Kind == Kind.Double ? x.Double : y.Double;
        var floatSource = x.Kind == Kind.Double ? x.Float : y.Float;
        if (double.IsNaN(f) || double.IsInfinity(f))
        {
            return false;
        }

        return FractionalMatch(d, f, floatSource);
    }

    private static bool FractionalMatch(decimal d, double f, bool isFloat)
    {
        try
        {
            if (isFloat)
            {
                return (float)d == (float)f && (decimal)(float)f == d;
            }

            return (double)d == f && (decimal)f == d;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static bool ExactEqual(Number fractional, Number whole)
    {
        if (fractional.Kind == Kind.Decimal)
        {
            return whole.Kind == Kind.Signed ? fractional.Decimal == whole.Signed : fractional.Decimal == whole.Unsigned;
        }

        var d = fractional.Double;
        if (double.IsNaN(d) || double.IsInfinity(d) || d != Math.Floor(d))
        {
            return false;
        }

        return whole.Kind == Kind.Signed
            ? d >= -9.223372036854775808E18 && d < 9.223372036854775808E18 && (long)d == whole.Signed
            : d >= 0 && d < 1.8446744073709552E19 && (ulong)d == whole.Unsigned;
    }

    public static bool IsNumber(object value) => TryRead(value, out _);

    public static int Hash(object value)
    {
        TryRead(value, out var n);
        float key;
        switch (n.Kind)
        {
            case Kind.Signed:
                key = (float)(double)n.Signed;
                break;
            case Kind.Unsigned:
                key = (float)(double)n.Unsigned;
                break;
            case Kind.Decimal:
                key = n.Decimal == decimal.Truncate(n.Decimal) && Math.Abs(n.Decimal) < 9.2e18m
                    ? (float)(double)(long)n.Decimal
                    : (float)(double)n.Decimal;
                break;
            default:
                key = (float)n.Double;
                break;
        }

        if (float.IsNaN(key))
        {
            return 1;
        }

        return key == 0f ? 0 : key.GetHashCode();
    }

    private static bool TryRead(object value, out Number number)
    {
        number = default;
        switch (value)
        {
            case sbyte v: number = new Number { Kind = Kind.Signed, Signed = v }; return true;
            case short v: number = new Number { Kind = Kind.Signed, Signed = v }; return true;
            case int v: number = new Number { Kind = Kind.Signed, Signed = v }; return true;
            case long v: number = new Number { Kind = Kind.Signed, Signed = v }; return true;
            case byte v: number = new Number { Kind = Kind.Unsigned, Unsigned = v }; return true;
            case ushort v: number = new Number { Kind = Kind.Unsigned, Unsigned = v }; return true;
            case uint v: number = new Number { Kind = Kind.Unsigned, Unsigned = v }; return true;
            case ulong v: number = new Number { Kind = Kind.Unsigned, Unsigned = v }; return true;
            case decimal v: number = new Number { Kind = Kind.Decimal, Decimal = v }; return true;
            case float v: number = new Number { Kind = Kind.Double, Double = v, Float = true }; return true;
            case double v: number = new Number { Kind = Kind.Double, Double = v }; return true;
            default: return false;
        }
    }

    private enum Kind
    {
        Signed,
        Unsigned,
        Decimal,
        Double,
    }

    private struct Number
    {
        public Kind Kind;
        public long Signed;
        public ulong Unsigned;
        public decimal Decimal;
        public double Double;
        public bool Float;
    }
}
