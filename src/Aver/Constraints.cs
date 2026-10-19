namespace Aver;

public readonly struct AndConstraint<TAssertions>
{
    public AndConstraint(TAssertions and) => And = and;

    public TAssertions And { get; }
}

public readonly struct AndWhichConstraint<TAssertions, TWhich>
{
    public AndWhichConstraint(TAssertions and, TWhich which)
    {
        And = and;
        Which = which;
    }

    public TAssertions And { get; }

    public TWhich Which { get; }

    public TWhich Subject => Which;
}
