using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace Aver;

public abstract class CollectionAssertionsBase<T, TSelf>
    where TSelf : CollectionAssertionsBase<T, TSelf>
{
    private readonly string? _expression;

    protected CollectionAssertionsBase(IEnumerable<T>? subject, string? expression)
    {
        Subject = subject switch
        {
            null => null,
            IReadOnlyList<T> list => list,
            _ => subject.ToList(),
        };
        _expression = expression;
    }

    public IReadOnlyList<T>? Subject { get; }

    protected string Name => Fail.Name(_expression);

    [DoesNotReturn]
    private void FailNull(string expectation) => Fail.With($"Expected {Name} {expectation}, but found null.");

    protected IReadOnlyList<T> Required(string expectation)
    {
        if (Subject is null)
        {
            FailNull(expectation);
        }

        return Subject;
    }

    protected AndConstraint<TSelf> And() => new((TSelf)this);

    public AndConstraint<TSelf> BeEmpty()
    {
        var items = Required("to be empty");
        if (items.Count != 0)
        {
            Fail.With($"Expected {Name} to be empty, but found {Plural(items.Count)}:{Lines(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> NotBeEmpty()
    {
        if (Required("not to be empty").Count == 0)
        {
            Fail.With($"Expected {Name} not to be empty, but it was.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeNull()
    {
        if (Subject is not null)
        {
            Fail.With($"Expected {Name} to be null, but found {Formatter.Format(Subject)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> NotBeNull()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Name} not to be null, but it was.");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCount(int expected)
    {
        if (Subject is null)
        {
            FailNull($"to contain {Plural(expected)}");
        }

        var items = Subject;
        if (items.Count != expected)
        {
            Fail.With($"Expected {Name} to contain {Plural(expected)}, but found {items.Count}:{Lines(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCountGreaterThan(int expected)
    {
        if (Subject is null)
        {
            FailNull($"to contain more than {Plural(expected)}");
        }

        var items = Subject;
        if (items.Count <= expected)
        {
            Fail.With($"Expected {Name} to contain more than {Plural(expected)}, but found {items.Count}:{Lines(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCountGreaterThanOrEqualTo(int expected)
    {
        if (Subject is null)
        {
            FailNull($"to contain at least {Plural(expected)}");
        }

        var items = Subject;
        if (items.Count < expected)
        {
            Fail.With($"Expected {Name} to contain at least {Plural(expected)}, but found {items.Count}:{Lines(items)}");
        }

        return And();
    }

    public AndWhichConstraint<TSelf, T> Contain(T expected)
    {
        if (Subject is null)
        {
            FailNull($"to contain {Formatter.Format(expected)}");
        }

        var items = Subject;
        var index = IndexOf(items, expected);
        if (index < 0)
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(expected)}, but it did not. It has:{Lines(items)}");
        }

        return new((TSelf)this, items[index]);
    }

    public AndConstraint<TSelf> Contain(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return ContainAll(Materialise(expected));
    }

    public AndConstraint<TSelf> Contain(params T[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return ContainAll(expected);
    }

    private AndConstraint<TSelf> ContainAll(IReadOnlyList<T> expected)
    {
        if (Subject is null)
        {
            FailNull($"to contain all of {Formatter.Format(expected)}");
        }

        var items = Subject;
        List<T>? missing = null;
        for (var i = 0; i < expected.Count; i++)
        {
            if (IndexOf(items, expected[i]) < 0)
            {
                (missing ??= []).Add(expected[i]);
            }
        }

        if (missing is not null)
        {
            Fail.With($"Expected {Name} to contain all of {Formatter.Format(expected)}, but it is missing {Formatter.Format(missing)}. It has:{Lines(items)}");
        }

        return And();
    }

    public AndWhichConstraint<TSelf, T> Contain(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (Subject is null)
        {
            FailNull($"to contain an item matching {predicate.Body}");
        }

        var items = Subject;
        var match = predicate.Compile();
        for (var i = 0; i < items.Count; i++)
        {
            if (match(items[i]))
            {
                return new((TSelf)this, items[i]);
            }
        }

        Fail.With($"Expected {Name} to contain an item matching {predicate.Body}, but it did not. It has:{Lines(items)}");
        return default;
    }

    public AndConstraint<TSelf> NotContain(T unexpected)
    {
        if (Subject is null)
        {
            FailNull($"not to contain {Formatter.Format(unexpected)}");
        }

        var items = Subject;
        var index = IndexOf(items, unexpected);
        if (index >= 0)
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(unexpected)}, but found it at index {index}.");
        }

        return And();
    }

    public AndConstraint<TSelf> NotContain(IEnumerable<T> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        var other = Materialise(unexpected);
        if (Subject is null)
        {
            FailNull($"not to contain any of {Formatter.Format(other)}");
        }

        var items = Subject;
        List<T>? found = null;
        for (var i = 0; i < other.Count; i++)
        {
            if (IndexOf(items, other[i]) >= 0)
            {
                (found ??= []).Add(other[i]);
            }
        }

        if (found is not null)
        {
            Fail.With($"Expected {Name} not to contain any of {Formatter.Format(other)}, but found {Formatter.Format(found)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> NotContain(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (Subject is null)
        {
            FailNull($"not to contain any item matching {predicate.Body}");
        }

        var items = Subject;
        var matching = Where(items, predicate.Compile(), expected: true);
        if (matching is not null)
        {
            Fail.With($"Expected {Name} not to contain any item matching {predicate.Body}, but found:{Lines(items, matching)}");
        }

        return And();
    }

    public AndWhichConstraint<TSelf, T> ContainSingle()
    {
        var items = Required("to contain a single item");
        if (items.Count != 1)
        {
            Fail.With($"Expected {Name} to contain a single item, but found {items.Count}:{Lines(items)}");
        }

        return new((TSelf)this, items[0]);
    }

    public AndWhichConstraint<TSelf, T> ContainSingle(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (Subject is null)
        {
            FailNull($"to contain a single item matching {predicate.Body}");
        }

        var items = Subject;
        var matching = Where(items, predicate.Compile(), expected: true);
        if (matching is null)
        {
            Fail.With($"Expected {Name} to contain a single item matching {predicate.Body}, but found 0. It has:{Lines(items)}");
        }

        if (matching.Count != 1)
        {
            Fail.With($"Expected {Name} to contain a single item matching {predicate.Body}, but found {matching.Count}:{Lines(items, matching)}");
        }

        return new((TSelf)this, items[matching[0]]);
    }

    public AndConstraint<TSelf> OnlyContain(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (Subject is null)
        {
            FailNull($"to only contain items matching {predicate.Body}");
        }

        var items = Subject;
        var failing = Where(items, predicate.Compile(), expected: false);
        if (failing is not null)
        {
            Fail.With($"Expected {Name} to only contain items matching {predicate.Body}, but these did not:{Lines(items, failing)}");
        }

        return And();
    }

    public AndConstraint<TSelf> Equal(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualTo(Materialise(expected));
    }

    public AndConstraint<TSelf> Equal(params T[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualTo(expected);
    }

    private AndConstraint<TSelf> EqualTo(IReadOnlyList<T> expected)
    {
        var items = Required("to equal the expected items");
        var comparer = EqualityComparer<T>.Default;
        var common = Math.Min(items.Count, expected.Count);
        var differ = -1;
        for (var i = 0; i < common; i++)
        {
            if (!comparer.Equals(items[i], expected[i]))
            {
                differ = i;
                break;
            }
        }

        if (differ < 0 && items.Count == expected.Count)
        {
            return And();
        }

        if (differ < 0)
        {
            differ = common;
        }

        var found = differ < items.Count ? Formatter.Format(items[differ]) : "nothing";
        var wanted = differ < expected.Count ? Formatter.Format(expected[differ]) : "nothing";
        var at = $"differ at index {differ}: expected {wanted}, found {found}.";
        if (items.Count != expected.Count)
        {
            Fail.With($"Expected {Name} to equal the expected items, but it has {Plural(items.Count)}, expected {expected.Count}.\n  They {at}");
        }

        Fail.With($"Expected {Name} to equal the expected items, but they {at}");
        return default!;
    }

    public AndConstraint<TSelf> NotEqual(IEnumerable<T> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        var other = Materialise(unexpected);
        if (Subject is null)
        {
            FailNull($"not to equal {Formatter.Format(other)}");
        }

        var items = Subject;
        if (items.Count != other.Count)
        {
            return And();
        }

        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < items.Count; i++)
        {
            if (!comparer.Equals(items[i], other[i]))
            {
                return And();
            }
        }

        Fail.With($"Expected {Name} not to equal {Formatter.Format(other)}, but it did.");
        return default!;
    }

    public AndConstraint<TSelf> OnlyHaveUniqueItems()
    {
        var items = Required("to have unique items");
        var seen = new HashSet<T>();
        for (var i = 0; i < items.Count; i++)
        {
            if (seen.Add(items[i]))
            {
                continue;
            }

            var comparer = EqualityComparer<T>.Default;
            var times = 0;
            for (var j = 0; j < items.Count; j++)
            {
                if (comparer.Equals(items[j], items[i]))
                {
                    times++;
                }
            }

            Fail.With($"Expected {Name} to have unique items, but {Formatter.Format(items[i])} appears {times} times.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeInAscendingOrder() => InOrder(Comparer<T>.Default, "ascending", false);

    public AndConstraint<TSelf> BeInAscendingOrder<TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return InOrder(Comparer<TKey>.Default, keySelector, "ascending", false);
    }

    public AndConstraint<TSelf> BeInDescendingOrder() => InOrder(Comparer<T>.Default, "descending", true);

    public AndConstraint<TSelf> BeInDescendingOrder<TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return InOrder(Comparer<TKey>.Default, keySelector, "descending", true);
    }

    private AndConstraint<TSelf> InOrder(IComparer<T> comparer, string direction, bool descending) =>
        InOrder(comparer, static x => x, direction, descending);

    private AndConstraint<TSelf> InOrder<TKey>(IComparer<TKey> comparer, Func<T, TKey> key, string direction, bool descending)
    {
        if (Subject is null)
        {
            FailNull($"to be in {direction} order");
        }

        var items = Subject;
        if (items.Count < 2)
        {
            return And();
        }

        var previous = key(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var current = key(items[i]);
            var order = comparer.Compare(previous, current);
            if (descending ? order < 0 : order > 0)
            {
                Fail.With($"Expected {Name} to be in {direction} order, but [{i}] {Formatter.Format(items[i])} comes after [{i - 1}] {Formatter.Format(items[i - 1])}.");
            }

            previous = current;
        }

        return And();
    }

    public AndConstraint<TSelf> AllSatisfy(Action<T> assertion)
    {
        ArgumentNullException.ThrowIfNull(assertion);
        var items = Required("to have all items satisfy the assertion");
        List<(int Index, string Message)>? failures = null;
        for (var i = 0; i < items.Count; i++)
        {
            try
            {
                assertion(items[i]);
            }
            catch (AverException ex)
            {
                (failures ??= []).Add((i, ex.Message));
            }
        }

        if (failures is not null)
        {
            var sb = new StringBuilder();
            sb.Append("Expected ").Append(Name).Append(" to have all items satisfy the assertion, but ").Append(failures.Count).Append(" did not:");
            var shown = Math.Min(failures.Count, Formatter.MaxItems);
            for (var i = 0; i < shown; i++)
            {
                sb.Append("\n  [").Append(failures[i].Index).Append("] ").Append(failures[i].Message);
            }

            AppendMore(sb, failures.Count - shown);
            Fail.With(sb.ToString());
        }

        return And();
    }

    public AndConstraint<TSelf> NotIntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var expected = Materialise(other);
        if (Subject is null)
        {
            FailNull($"not to intersect with {Formatter.Format(expected)}");
        }

        var items = Subject;
        var set = new HashSet<T>(expected);
        var seen = new HashSet<T>();
        List<T>? common = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (set.Contains(items[i]) && seen.Add(items[i]))
            {
                (common ??= []).Add(items[i]);
            }
        }

        if (common is not null)
        {
            Fail.With($"Expected {Name} not to intersect with {Formatter.Format(expected)}, but found common items {Formatter.Format(common)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeSubsetOf(IEnumerable<T> superset)
    {
        ArgumentNullException.ThrowIfNull(superset);
        var expected = Materialise(superset);
        if (Subject is null)
        {
            FailNull($"to be a subset of {Formatter.Format(expected)}");
        }

        var items = Subject;
        var set = new HashSet<T>(expected);
        var seen = new HashSet<T>();
        List<T>? extra = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (!set.Contains(items[i]) && seen.Add(items[i]))
            {
                (extra ??= []).Add(items[i]);
            }
        }

        if (extra is not null)
        {
            Fail.With($"Expected {Name} to be a subset of {Formatter.Format(expected)}, but {Formatter.Format(extra)} are not in it.");
        }

        return And();
    }

    private static IReadOnlyList<T> Materialise(IEnumerable<T> source) =>
        source as IReadOnlyList<T> ?? source.ToList();

    private static int IndexOf(IReadOnlyList<T> items, T value)
    {
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < items.Count; i++)
        {
            if (comparer.Equals(items[i], value))
            {
                return i;
            }
        }

        return -1;
    }

    private static List<int>? Where(IReadOnlyList<T> items, Func<T, bool> predicate, bool expected)
    {
        List<int>? indexes = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (predicate(items[i]) == expected)
            {
                (indexes ??= []).Add(i);
            }
        }

        return indexes;
    }

    protected static string Plural(int count) => count == 1 ? "1 item" : $"{count} items";

    protected static string Lines(IReadOnlyList<T> items, List<int>? indexes = null)
    {
        var total = indexes?.Count ?? items.Count;
        var shown = Math.Min(total, Formatter.MaxItems);
        var sb = new StringBuilder();
        for (var i = 0; i < shown; i++)
        {
            var index = indexes?[i] ?? i;
            sb.Append("\n  [").Append(index).Append("] ").Append(Formatter.Format(items[index]));
        }

        AppendMore(sb, total - shown);
        return sb.ToString();
    }

    private static void AppendMore(StringBuilder sb, int more)
    {
        if (more > 0)
        {
            sb.Append("\n  ... (").Append(more).Append(" more)");
        }
    }
}
