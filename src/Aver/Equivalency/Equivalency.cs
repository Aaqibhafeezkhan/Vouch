using System.Collections;
using System.Reflection;
using System.Text;

namespace Aver;

internal static class Equivalency
{
    internal const int MaxDepth = 10;
    internal const int MaxListed = 10;

    public static int Compare(object? subject, object? expectation, EquivalencyOptions options, List<Difference> differences, int maxListed = int.MaxValue)
    {
        var run = new Comparison(options, differences, maxListed, stopOnFirst: false);
        run.CompareValues(subject, expectation);
        return run.Total;
    }

    public static bool AreEquivalent(object? subject, object? expectation, EquivalencyOptions options)
    {
        var run = new Comparison(options, null, 0, stopOnFirst: true);
        run.CompareValues(subject, expectation);
        return run.Total == 0;
    }

    public static void Require(object? subject, object? expectation, Type expectedType, EquivalencyOptions options, string name)
    {
        var differences = new List<Difference>();
        var total = Compare(subject, expectation, options, differences, MaxListed);
        if (total > 0)
        {
            Fail.With(Message(name, expectation?.GetType() ?? expectedType, differences, total));
        }
    }

    public static bool IsCollection(Type type) =>
        MemberCache.ShapeOf(type).Kind is TypeKind.Sequence or TypeKind.Dictionary;

    public static bool ElementsAreCollections(Type sequence)
    {
        foreach (var contract in sequence.GetInterfaces())
        {
            if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return IsCollection(contract.GetGenericArguments()[0]);
            }
        }

        return false;
    }

    public static bool IsDictionary(object value) =>
        MemberCache.ShapeOf(value.GetType()).Kind == TypeKind.Dictionary;

    public static EquivalencyOptions Options(Func<EquivalencyOptions, EquivalencyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        return EquivalencyOptions.Resolve(configure);
    }

    private static string Message(string name, Type expectedType, List<Difference> differences, int total)
    {
        var sb = new StringBuilder("Expected ").Append(name).Append(" to be equivalent to the expected ")
            .Append(Formatter.IsAnonymous(expectedType) ? "anonymous type" : Formatter.TypeName(expectedType)).Append(", but:");
        for (var i = 0; i < differences.Count; i++)
        {
            sb.Append("\n  ");
            if (differences[i].Path.Length > 0)
            {
                sb.Append(differences[i].Path).Append(' ');
            }

            sb.Append(differences[i].Description).Append('.');
        }

        if (total > differences.Count)
        {
            sb.Append("\n  ... and ").Append(total - differences.Count).Append(" more differences");
        }

        return sb.ToString();
    }
}

internal sealed class Comparison
{
    private enum Segment : byte
    {
        Member,
        Index,
        Key,
    }

    private enum Kind : byte
    {
        Mismatch,
        NotFound,
        Count,
        NoMember,
        NoKey,
        UnexpectedKey,
        Cycle,
        Depth,
        Threw,
    }

    private struct Step
    {
        public Segment Kind;
        public string? Name;
        public int Index;
        public object? Key;
    }

    private readonly List<Difference>? _differences;
    private readonly int _maxListed;
    private readonly List<Step> _path = new();
    private readonly List<object> _pairs = new();

    private bool _quiet;
    private bool _stopOnFirst;

    public Comparison(EquivalencyOptions options, List<Difference>? differences, int maxListed, bool stopOnFirst)
    {
        Options = options;
        _differences = differences;
        _maxListed = maxListed;
        _stopOnFirst = stopOnFirst;
        _quiet = differences is null;
    }

    public EquivalencyOptions Options { get; }

    public int Total { get; private set; }

    public bool Stop => _stopOnFirst && Total > 0;

    public void CompareValues(object? subject, object? expectation)
    {
        if (expectation is null)
        {
            if (subject is not null)
            {
                Add(Kind.Mismatch, subject, expectation);
            }

            return;
        }

        if (subject is null)
        {
            Add(Kind.Mismatch, subject, expectation);
            return;
        }

        if (ReferenceEquals(subject, expectation))
        {
            return;
        }

        var shape = MemberCache.ShapeOf(expectation.GetType());
        switch (shape.Kind)
        {
            case TypeKind.Scalar:
                if (!ScalarEquals(subject, expectation))
                {
                    Add(Kind.Mismatch, subject, expectation);
                }

                return;
            case TypeKind.Equatable:
                CompareEquatable(subject, expectation);
                return;
            default:
                if (!Compatible(subject, shape.Kind))
                {
                    Add(Kind.Mismatch, subject, expectation);
                    return;
                }

                CompareStructure(subject, expectation, shape);
                return;
        }
    }

    public bool Probe(object? subject, object? expectation, int index)
    {
        var total = Total;
        var quiet = _quiet;
        var stop = _stopOnFirst;
        var pathCount = _path.Count;
        var pairCount = _pairs.Count;
        _quiet = true;
        _stopOnFirst = true;
        Total = 0;
        Push(index);
        CompareValues(subject, expectation);
        var equivalent = Total == 0;
        Restore(pathCount, pairCount);
        Total = total;
        _quiet = quiet;
        _stopOnFirst = stop;
        return equivalent;
    }

    public void ReportNotFound(int index, object? expected)
    {
        Push(index);
        Add(Kind.NotFound, null, expected);
        _path.RemoveAt(_path.Count - 1);
    }

    private static bool Compatible(object subject, TypeKind expected)
    {
        var actual = MemberCache.ShapeOf(subject.GetType()).Kind;
        return expected switch
        {
            TypeKind.Dictionary => actual == TypeKind.Dictionary,
            TypeKind.Sequence => actual == TypeKind.Sequence,
            _ => actual != TypeKind.Scalar,
        };
    }

    private void CompareEquatable(object subject, object expectation)
    {
        try
        {
            if (!subject.Equals(expectation))
            {
                Add(Kind.Mismatch, subject, expectation);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            AddThrew(ex);
        }
    }

    private void CompareStructure(object subject, object expectation, TypeShape shape)
    {
        var pathCount = _path.Count;
        var pairCount = _pairs.Count;
        try
        {
            if (pathCount > Equivalency.MaxDepth)
            {
                Add(Kind.Depth, null, null);
                return;
            }

            for (var i = 0; i < pairCount; i += 2)
            {
                if (ReferenceEquals(_pairs[i], subject) && ReferenceEquals(_pairs[i + 1], expectation))
                {
                    Add(Kind.Cycle, null, null);
                    return;
                }
            }

            _pairs.Add(subject);
            _pairs.Add(expectation);
            switch (shape.Kind)
            {
                case TypeKind.Dictionary:
                    CompareDictionaries(subject, expectation);
                    break;
                case TypeKind.Sequence:
                    CompareSequences(subject, expectation);
                    break;
                default:
                    CompareMembers(subject, expectation, shape);
                    break;
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Restore(pathCount, pairCount);
            AddThrew(ex);
        }
        finally
        {
            if (_pairs.Count > pairCount)
            {
                _pairs.RemoveRange(pairCount, _pairs.Count - pairCount);
            }
        }
    }

    private void CompareMembers(object subject, object expectation, TypeShape shape)
    {
        var subjectShape = MemberCache.ShapeOf(subject.GetType());
        var members = shape.Members;
        for (var i = 0; i < members.Length && !Stop; i++)
        {
            var member = members[i];
            var counterpart = subjectShape.Find(member.Name);
            if (counterpart is null)
            {
                Add(Kind.NoMember, null, null, member.Name);
                continue;
            }

            var pathCount = _path.Count;
            _path.Add(new Step { Kind = Segment.Member, Name = member.Name });
            try
            {
                var actual = counterpart.Get(subject);
                var expected = member.Get(expectation);
                CompareValues(actual, expected);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                AddThrew(ex);
            }

            _path.RemoveRange(pathCount, _path.Count - pathCount);
        }
    }

    private void CompareSequences(object subject, object expectation)
    {
        var actual = ToArray(subject);
        var expected = ToArray(expectation);
        if (actual.Length != expected.Length)
        {
            Add(Kind.Count, actual.Length, expected.Length);
            return;
        }

        if (Options.StrictOrdering)
        {
            for (var i = 0; i < expected.Length && !Stop; i++)
            {
                var pathCount = _path.Count;
                Push(i);
                CompareValues(actual[i], expected[i]);
                _path.RemoveRange(pathCount, _path.Count - pathCount);
            }

            return;
        }

        AnyOrderMatcher.Run(this, actual, expected);
    }

    private void CompareDictionaries(object subject, object expectation)
    {
        var actual = DictionaryView.Of(subject);
        var expected = DictionaryView.Of(expectation);
        var missing = false;
        foreach (var entry in expected.Entries())
        {
            if (Stop)
            {
                return;
            }

            if (!actual.TryGetValue(entry.Key, out var value))
            {
                missing = true;
                Add(Kind.NoKey, entry.Key, null);
                continue;
            }

            var pathCount = _path.Count;
            _path.Add(new Step { Kind = Segment.Key, Key = entry.Key });
            CompareValues(value, entry.Value);
            _path.RemoveRange(pathCount, _path.Count - pathCount);
        }

        if (missing || actual.Count > expected.Count)
        {
            foreach (var entry in actual.Entries())
            {
                if (Stop)
                {
                    return;
                }

                if (!expected.TryGetValue(entry.Key, out _))
                {
                    Add(Kind.UnexpectedKey, entry.Key, null);
                }
            }
        }
    }

    private static bool ScalarEquals(object subject, object expectation)
    {
        if (subject.Equals(expectation))
        {
            return true;
        }

        if (subject is Enum a && expectation is Enum b && a.GetType() != b.GetType())
        {
            return Convert.ToDecimal(a) == Convert.ToDecimal(b);
        }

        return false;
    }

    private static object?[] ToArray(object sequence)
    {
        if (sequence is object?[] direct)
        {
            return direct;
        }

        if (sequence is ICollection collection)
        {
            var copy = new object?[collection.Count];
            collection.CopyTo(copy, 0);
            return copy;
        }

        var items = new List<object?>();
        foreach (var item in (IEnumerable)sequence)
        {
            items.Add(item);
        }

        return items.ToArray();
    }

    private void Push(int index) => _path.Add(new Step { Kind = Segment.Index, Index = index });

    private void Restore(int pathCount, int pairCount)
    {
        if (_path.Count > pathCount)
        {
            _path.RemoveRange(pathCount, _path.Count - pathCount);
        }

        if (_pairs.Count > pairCount)
        {
            _pairs.RemoveRange(pairCount, _pairs.Count - pairCount);
        }
    }

    private void AddThrew(Exception ex) => Add(Kind.Threw, ex is TargetInvocationException { InnerException: { } inner } ? inner : ex, null);

    private void Add(Kind kind, object? actual, object? expected, string? name = null)
    {
        Total++;
        if (_quiet || _differences is null || _differences.Count >= _maxListed)
        {
            return;
        }

        _differences.Add(new Difference(PathText(), Describe(kind, actual, expected, name)));
    }

    private static string Describe(Kind kind, object? actual, object? expected, string? name) => kind switch
    {
        Kind.Mismatch => $"was {Formatter.Format(actual)}, expected {Formatter.Format(expected)}",
        Kind.NotFound => $"expected {Formatter.Format(expected)} was not found",
        Kind.Count => $"has {Items((int)actual!)}, expected {Items((int)expected!)}",
        Kind.NoMember => $"has no member {name}",
        Kind.NoKey => $"has no key {Formatter.Format(actual)}",
        Kind.UnexpectedKey => $"has unexpected key {Formatter.Format(actual)}",
        Kind.Cycle => "is a cyclic reference",
        Kind.Depth => $"is nested deeper than {Equivalency.MaxDepth} levels",
        _ => $"threw {actual!.GetType().Name}: {((Exception)actual).Message}",
    };

    private static string Items(int count) => count == 1 ? "1 item" : $"{count} items";

    private string PathText()
    {
        if (_path.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var step in _path)
        {
            switch (step.Kind)
            {
                case Segment.Member:
                    if (sb.Length > 0)
                    {
                        sb.Append('.');
                    }

                    sb.Append(step.Name);
                    break;
                case Segment.Index:
                    sb.Append('[').Append(step.Index).Append(']');
                    break;
                default:
                    sb.Append('[').Append(Formatter.Format(step.Key)).Append(']');
                    break;
            }
        }

        return sb.ToString();
    }
}
