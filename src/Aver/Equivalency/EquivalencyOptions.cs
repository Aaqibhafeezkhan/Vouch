namespace Aver;

public sealed class EquivalencyOptions
{
    internal static readonly EquivalencyOptions Default = new();

    internal bool StrictOrdering { get; private init; }

    public EquivalencyOptions WithStrictOrdering() => new() { StrictOrdering = true };

    internal static EquivalencyOptions Resolve(Func<EquivalencyOptions, EquivalencyOptions>? configure)
    {
        if (configure is null)
        {
            return Default;
        }

        return configure(Default) ?? throw new InvalidOperationException("The configure callback returned null.");
    }
}
