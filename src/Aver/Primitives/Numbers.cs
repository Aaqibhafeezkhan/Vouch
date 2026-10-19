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

        return x.Kind switch
        {
            Kind.Signed when y.Kind == Kind.Signed => x.Signed == y.Signed,
            Kind.Unsigned when y.Kind == Kind.Unsigned => x.Unsigned == y.Unsigned,
            Kind.Signed when y.Kind == Kind.Unsigned => x.Signed >= 0 && (ulong)x.Signed == y.Unsigned,
            Kind.Unsigned when y.Kind == Kind.Signed => y.Signed >= 0 && (ulong)y.Signed == x.Unsigned,
            Kind.Decimal when y.Kind == Kind.Decimal => x.Decimal == y.Decimal,
            Kind.Decimal when y.Kind == Kind.Signed => x.Decimal == y.Signed,
            Kind.Decimal when y.Kind == Kind.Unsigned => x.Decimal == y.Unsigned,
            Kind.Signed when y.Kind == Kind.Decimal => x.Signed == y.Decimal,
            Kind.Unsigned when y.Kind == Kind.Decimal => x.Unsigned == y.Decimal,
            Kind.Decimal => (double)x.Decimal == y.Double,
            Kind.Double when y.Kind == Kind.Decimal => x.Double == (double)y.Decimal,
            Kind.Double when y.Kind == Kind.Double => x.Double == y.Double,
            Kind.Double when y.Kind == Kind.Signed => EqualsSigned(x.Double, y.Signed),
            Kind.Double => EqualsUnsigned(x.Double, y.Unsigned),
            Kind.Signed => EqualsSigned(y.Double, x.Signed),
            _ => EqualsUnsigned(y.Double, x.Unsigned),
        };
    }

    public static bool IsNumber(object value) => TryRead(value, out _);

    public static int Hash(object value)
    {
        TryRead(value, out var n);
        return n.Kind switch
        {
            Kind.Signed => n.Signed.GetHashCode(),
            Kind.Unsigned => n.Unsigned <= long.MaxValue ? ((long)n.Unsigned).GetHashCode() : ((double)n.Unsigned).GetHashCode(),
            Kind.Decimal => HashDouble((double)n.Decimal),
            _ => HashDouble(n.Double),
        };
    }

    private static int HashDouble(double d)
        => d == Math.Floor(d) && d >= -9.223372036854775808E18 && d < 9.223372036854775808E18
            ? ((long)d).GetHashCode()
            : d.GetHashCode();

    private static bool EqualsSigned(double d, long s)
        => d == Math.Floor(d) && d >= -9.223372036854775808E18 && d < 9.223372036854775808E18 && (long)d == s;

    private static bool EqualsUnsigned(double d, ulong u)
        => d == Math.Floor(d) && d >= 0 && d < 1.8446744073709552E19 && (ulong)d == u;

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
            case float v: number = new Number { Kind = Kind.Double, Double = v }; return true;
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
    }
}
