namespace Vouch;

public sealed class GenericCollectionAssertions<T> : CollectionAssertionsBase<T, GenericCollectionAssertions<T>>
{
    internal GenericCollectionAssertions(IEnumerable<T>? subject, string? expression)
        : base(subject, expression)
    {
    }
}
