namespace Aver;

public readonly struct ObjectAssertions
{
    private readonly string? _expression;

    internal ObjectAssertions(object? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public object? Subject { get; }

    public AndConstraint<ObjectAssertions> Be(object? expected)
    {
        if (!object.Equals(Subject, expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBe(object? unexpected)
    {
        if (object.Equals(Subject, unexpected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeNull()
    {
        if (Subject is not null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBeNull()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
        }

        return new(this);
    }
}
