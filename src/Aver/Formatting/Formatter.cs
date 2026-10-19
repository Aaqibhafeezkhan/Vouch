using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Aver;

internal static class Formatter
{
    public const int MaxDepth = 3;
    public const int MaxItems = 10;
    public const int MaxStringLength = 200;

    private const int MaxCountedRemainder = 100_000;
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static string Format(object? value)
    {
        var sb = new StringBuilder();
        try
        {
            Write(sb, value, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }
        catch (Exception ex)
        {
            return $"<threw {ex.GetType().Name}>";
        }

        return sb.ToString();
    }

    private static void Write(StringBuilder sb, object? value, int depth, HashSet<object> path)
    {
        switch (value)
        {
            case null: sb.Append("null"); return;
            case string s: WriteString(sb, s); return;
            case bool b: sb.Append(b ? "true" : "false"); return;
            case char c: sb.Append('\'').Append(c == '\'' ? "\\'" : Escape(c.ToString())).Append('\''); return;
            case decimal m: sb.Append(m.ToString(Invariant)).Append('m'); return;
            case double d: sb.Append(d.ToString("R", Invariant)); return;
            case float f: sb.Append(f.ToString("R", Invariant)).Append('f'); return;
            case long l: sb.Append(l.ToString(Invariant)).Append('L'); return;
            case int or short or byte or sbyte or ushort or uint or ulong:
                sb.Append(((IFormattable)value).ToString(null, Invariant)); return;
            case DateTime dt: sb.Append(dt.ToString("o", Invariant)); return;
            case DateTimeOffset dto: sb.Append(dto.ToString("o", Invariant)); return;
            case DateOnly dateOnly: sb.Append(dateOnly.ToString("yyyy-MM-dd", Invariant)); return;
            case TimeSpan ts: sb.Append(ts.ToString("c", Invariant)); return;
            case Guid g: sb.Append(g.ToString("D", Invariant)); return;
            case Enum e: sb.Append(TypeName(e.GetType())).Append('.').Append(e.ToString()); return;
            case Type t: sb.Append(TypeName(t)); return;
        }

        var type = value.GetType();
        var isReference = !type.IsValueType;
        if (isReference && !path.Add(value))
        {
            sb.Append("<cycle ").Append(TypeName(type)).Append('>');
            return;
        }

        try
        {
            if (value is IDictionary dictionary)
            {
                WriteDictionary(sb, dictionary, depth, path);
            }
            else if (value is IEnumerable enumerable)
            {
                WriteSequence(sb, enumerable, depth, path);
            }
            else if (!IsRecord(type) && !IsAnonymous(type) && OverridesToString(type))
            {
                WriteToString(sb, value);
            }
            else
            {
                WriteObject(sb, value, type, depth, path);
            }
        }
        finally
        {
            if (isReference)
            {
                path.Remove(value);
            }
        }
    }

    private static void WriteString(StringBuilder sb, string s)
    {
        sb.Append('"');
        if (s.Length > MaxStringLength)
        {
            sb.Append(Escape(s.Substring(0, MaxStringLength)));
            sb.Append("... (").Append(s.Length - MaxStringLength).Append(" more chars)");
        }
        else
        {
            sb.Append(Escape(s));
        }

        sb.Append('"');
    }

    private static string Escape(string s)
    {
        StringBuilder? sb = null;
        for (var i = 0; i < s.Length; i++)
        {
            var replacement = s[i] switch
            {
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '"' => "\\\"",
                '\\' => "\\\\",
                _ => null,
            };

            if (replacement is null)
            {
                sb?.Append(s[i]);
                continue;
            }

            sb ??= new StringBuilder(s.Length + 8).Append(s, 0, i);
            sb.Append(replacement);
        }

        return sb?.ToString() ?? s;
    }

    private static void WriteSequence(StringBuilder sb, IEnumerable sequence, int depth, HashSet<object> path)
    {
        if (depth >= MaxDepth)
        {
            sb.Append("[...]");
            return;
        }

        sb.Append('[');
        try
        {
            var enumerator = sequence.GetEnumerator();
            try
            {
                var count = 0;
                while (count < MaxItems && enumerator.MoveNext())
                {
                    if (count > 0)
                    {
                        sb.Append(", ");
                    }

                    Write(sb, enumerator.Current, depth + 1, path);
                    count++;
                }

                WriteRemainder(sb, enumerator, count);
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
        }

        sb.Append(']');
    }

    private static void WriteDictionary(StringBuilder sb, IDictionary dictionary, int depth, HashSet<object> path)
    {
        if (depth >= MaxDepth)
        {
            sb.Append("{...}");
            return;
        }

        sb.Append('{');
        try
        {
            var enumerator = dictionary.GetEnumerator();
            try
            {
                var count = 0;
                while (count < MaxItems && enumerator.MoveNext())
                {
                    if (count > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append('[');
                    Write(sb, enumerator.Key, depth + 1, path);
                    sb.Append("] = ");
                    Write(sb, enumerator.Value, depth + 1, path);
                    count++;
                }

                WriteRemainder(sb, enumerator, count);
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
        }

        sb.Append('}');
    }

    private static void WriteRemainder(StringBuilder sb, IEnumerator enumerator, int shown)
    {
        if (shown < MaxItems)
        {
            return;
        }

        var more = 0;
        while (more < MaxCountedRemainder && enumerator.MoveNext())
        {
            more++;
        }

        if (more == 0)
        {
            return;
        }

        sb.Append(", ... (").Append(more).Append(more == MaxCountedRemainder ? "+ more)" : " more)");
    }

    private static void WriteToString(StringBuilder sb, object value)
    {
        try
        {
            sb.Append(value.ToString() ?? "null");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
        }
    }

    private static void WriteObject(StringBuilder sb, object value, Type type, int depth, HashSet<object> path)
    {
        var anonymous = IsAnonymous(type);
        if (!anonymous)
        {
            sb.Append(TypeName(type)).Append(' ');
        }

        if (depth >= MaxDepth)
        {
            sb.Append("{ ... }");
            return;
        }

        var members = MembersOf(type);
        if (members.Count == 0)
        {
            sb.Append("{ }");
            return;
        }

        sb.Append("{ ");
        for (var i = 0; i < members.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(members[i].Name).Append(" = ");
            object? memberValue;
            try
            {
                memberValue = members[i] is PropertyInfo p ? p.GetValue(value) : ((FieldInfo)members[i]).GetValue(value);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var inner = ex is TargetInvocationException { InnerException: { } cause } ? cause : ex;
                sb.Append("<threw ").Append(inner.GetType().Name).Append('>');
                continue;
            }

            Write(sb, memberValue, depth + 1, path);
        }

        sb.Append(" }");
    }

    private static List<MemberInfo> MembersOf(Type type)
    {
        var members = new List<MemberInfo>();
        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetIndexParameters().Length == 0 && p.GetMethod is not null)
            {
                members.Add(p);
            }
        }

        members.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
        return members;
    }

    private static bool OverridesToString(Type type)
    {
        var declaring = type.GetMethod(nameof(ToString), Type.EmptyTypes)?.DeclaringType;
        return declaring is not null && declaring != typeof(object) && declaring != typeof(ValueType);
    }

    private static bool IsAnonymous(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.Name.Contains("AnonymousType", StringComparison.Ordinal);

    private static bool IsRecord(Type type)
    {
        const BindingFlags all = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return type.GetProperty("EqualityContract", all) is not null
            || type.GetMethod("<Clone>$", all) is not null
            || (type.IsValueType && type.GetMethod("PrintMembers", all, null, [typeof(StringBuilder)], null) is not null);
    }

    private static string TypeName(Type type)
    {
        if (type.IsArray)
        {
            return TypeName(type.GetElementType()!) + "[]";
        }

        var name = type.Name;
        if (!type.IsGenericType)
        {
            return name;
        }

        var tick = name.IndexOf('`');
        if (tick >= 0)
        {
            name = name.Substring(0, tick);
        }

        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(TypeName)) + ">";
    }
}
