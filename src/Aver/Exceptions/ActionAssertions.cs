namespace Aver;

public readonly struct ActionAssertions
{
    private readonly string? _expression;

    internal ActionAssertions(Action? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Action? Subject { get; }

    public ExceptionAssertions<TException> Throw<TException>() where TException : Exception
    {
        var name = Fail.Name(_expression);
        if (Subject is null)
        {
            ExceptionMessages.NullDelegate(name, $"to throw {Formatter.Format(typeof(TException))}");
        }

        return ExceptionMessages.Check<TException>(name, Capture.Run(Subject!));
    }

    public AndConstraint<ActionAssertions> NotThrow()
    {
        var name = Fail.Name(_expression);
        if (Subject is null)
        {
            ExceptionMessages.NullDelegate(name, "not to throw");
        }

        var thrown = Capture.Run(Subject!);
        if (thrown is not null)
        {
            ExceptionMessages.Threw(name, thrown);
        }

        return new(this);
    }
}

public readonly struct FunctionAssertions<T>
{
    private readonly ActionAssertions _inner;

    internal FunctionAssertions(Func<T>? subject, string? expression)
    {
        Subject = subject;
        _inner = new ActionAssertions(subject is null ? null : () => subject(), expression);
    }

    public Func<T>? Subject { get; }

    public ExceptionAssertions<TException> Throw<TException>() where TException : Exception
        => _inner.Throw<TException>();

    public AndConstraint<FunctionAssertions<T>> NotThrow()
    {
        _inner.NotThrow();
        return new(this);
    }
}

internal static class Capture
{
    public static Exception? Run(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
