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

    private static string Word(bool? value) => value is null ? "null" : value.Value ? "true" : "false";
}
