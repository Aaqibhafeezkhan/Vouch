namespace Aver;

public sealed class DictionaryAssertions<TKey, TValue>
    : CollectionAssertionsBase<KeyValuePair<TKey, TValue>, DictionaryAssertions<TKey, TValue>>
    where TKey : notnull
{
    private readonly IDictionary<TKey, TValue>? _mutable;
    private readonly IReadOnlyDictionary<TKey, TValue>? _readOnly;

    internal DictionaryAssertions(IDictionary<TKey, TValue>? subject, string? expression)
        : base(subject, expression) => _mutable = subject;

    internal DictionaryAssertions(IReadOnlyDictionary<TKey, TValue>? subject, string? expression)
        : base(subject, expression) => _readOnly = subject;

    public AndWhichConstraint<DictionaryAssertions<TKey, TValue>, TValue> ContainKey(TKey expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        var items = Subject;
        if (items is null)
        {
            Fail.With($"Expected {Name} to contain key {Formatter.Format(expected)}, but found null.");
        }

        if (!TryGetValue(expected, out var value))
        {
            Fail.With($"Expected {Name} to contain key {Formatter.Format(expected)}, but it did not. It has:{Lines(items)}");
        }

        return new(this, value);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainKey(TKey unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        if (Subject is null)
        {
            Fail.With($"Expected {Name} not to contain key {Formatter.Format(unexpected)}, but found null.");
        }

        if (TryGetValue(unexpected, out var value))
        {
            Fail.With($"Expected {Name} not to contain key {Formatter.Format(unexpected)}, but found it with value {Formatter.Format(value)}.");
        }

        return And();
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainKeys(params TKey[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        foreach (var key in expected)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(expected));
        }

        var items = Subject;
        if (items is null)
        {
            Fail.With($"Expected {Name} to contain keys {Formatter.Format(expected)}, but found null.");
        }

        List<TKey>? missing = null;
        foreach (var key in expected)
        {
            if (!TryGetValue(key, out _))
            {
                (missing ??= []).Add(key);
            }
        }

        if (missing is not null)
        {
            Fail.With($"Expected {Name} to contain keys {Formatter.Format(expected)}, but it is missing {Formatter.Format(missing)}. It has:{Lines(items)}");
        }

        return And();
    }

    private bool TryGetValue(TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
        if (_mutable is not null)
        {
            return _mutable.TryGetValue(key, out value);
        }

        return _readOnly!.TryGetValue(key, out value);
    }
}
