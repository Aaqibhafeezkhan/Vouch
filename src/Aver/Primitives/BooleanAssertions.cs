namespace Aver;

public readonly struct BooleanAssertions
{
    private readonly string? _expression;

    internal BooleanAssertions(bool subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public bool Subject { get; }

    public AndConstraint<BooleanAssertions> BeTrue() => Be(true);

    public AndConstraint<BooleanAssertions> BeFalse() => Be(false);

    public AndConstraint<BooleanAssertions> Be(bool expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Word(expected)}, but found {Word(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<BooleanAssertions> NotBe(bool unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Word(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<BooleanAssertions> BeTrue(string because)
    {
        try
        {
            return BeTrue();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<BooleanAssertions> BeFalse(string because)
    {
        try
        {
            return BeFalse();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<BooleanAssertions> Be(bool expected, string because)
    {
        try
        {
            return Be(expected);
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<BooleanAssertions> NotBe(bool unexpected, string because)
    {
        try
        {
            return NotBe(unexpected);
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    private static string Word(bool value) => value ? "true" : "false";
}

public readonly struct NullableBooleanAssertions
{
    private readonly string? _expression;

    internal NullableBooleanAssertions(bool? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public bool? Subject { get; }

    public AndConstraint<NullableBooleanAssertions> BeTrue() => Be(true);

    public AndConstraint<NullableBooleanAssertions> BeFalse() => Be(false);

    public AndConstraint<NullableBooleanAssertions> Be(bool expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Word(expected)}, but found {Word(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableBooleanAssertions> NotBe(bool unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Word(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableBooleanAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Word(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableBooleanAssertions, bool> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return default;
        }

        return new(this, value);
    }

    public AndConstraint<NullableBooleanAssertions> BeTrue(string because)
    {
        try
        {
            return BeTrue();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NullableBooleanAssertions> BeFalse(string because)
    {
        try
        {
            return BeFalse();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NullableBooleanAssertions> Be(bool expected, string because)
    {
        try
        {
            return Be(expected);
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NullableBooleanAssertions> NotBe(bool unexpected, string because)
    {
        try
        {
            return NotBe(unexpected);
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NullableBooleanAssertions> BeNull(string because)
    {
        try
        {
            return BeNull();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndWhichConstraint<NullableBooleanAssertions, bool> NotBeNull(string because)
    {
        try
        {
            return NotBeNull();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    private static string Word(bool? value) => value is null ? "null" : value.Value ? "true" : "false";
}
