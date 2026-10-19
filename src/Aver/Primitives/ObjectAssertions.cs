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

    public AndWhichConstraint<ObjectAssertions, T> BeOfType<T>()
    {
        if (Subject is null || Subject.GetType() != typeof(T))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be of type {Formatter.Format(typeof(T))}, but found {TypeOrNull()}.");
        }

        return new(this, (T)Subject);
    }

    public AndWhichConstraint<ObjectAssertions, T> BeAssignableTo<T>()
    {
        if (Subject is not T typed)
        {
            if (Subject is null)
            {
                Fail.With($"Expected {Fail.Name(_expression)} to be assignable to {Formatter.Format(typeof(T))}, but found null.");
            }

            Fail.With($"Expected {Fail.Name(_expression)} to be assignable to {Formatter.Format(typeof(T))}, but {Formatter.Format(Subject.GetType())} is not.");
            return default;
        }

        return new(this, typed);
    }

    public AndConstraint<ObjectAssertions> NotBeOfType<T>()
    {
        if (Subject is not null && Subject.GetType() == typeof(T))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be of type {Formatter.Format(typeof(T))}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBeAssignableTo<T>()
    {
        if (Subject is T)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be assignable to {Formatter.Format(typeof(T))}, but {Formatter.Format(Subject.GetType())} is.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeSameAs(object? expected)
    {
        if (!ReferenceEquals(Subject, expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to refer to {Formatter.Format(expected)}, but found another object: {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBeSameAs(object? unexpected)
    {
        if (ReferenceEquals(Subject, unexpected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to refer to {Formatter.Format(unexpected)}, but it did.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeOneOf(params object?[] validValues)
    {
        foreach (var value in validValues)
        {
            if (object.Equals(Subject, value))
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Fail.Name(_expression)} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => throw new NotImplementedException();

    private string TypeOrNull() => Subject is null ? "null" : Formatter.Format(Subject.GetType());
}
