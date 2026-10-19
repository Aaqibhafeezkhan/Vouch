using System.Runtime.CompilerServices;

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

        RejectAsyncVoid(Subject!);
        return ExceptionMessages.Check<TException>(name, Capture.Run(Subject!));
    }

    public AndConstraint<ActionAssertions> NotThrow()
    {
        var name = Fail.Name(_expression);
        if (Subject is null)
        {
            ExceptionMessages.NullDelegate(name, "not to throw");
        }

        RejectAsyncVoid(Subject!);
        var thrown = Capture.Run(Subject!);
        if (thrown is not null)
        {
            ExceptionMessages.Threw(name, thrown);
        }

        return new(this);
    }

    private static void RejectAsyncVoid(Action action)
    {
        if (action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false))
        {
            throw new InvalidOperationException("Cannot use action assertions on an async void method. Assign the async method to a variable of type Func<Task> instead of Action so that it can be awaited.");
        }
    }

    public ExceptionAssertions<TException> Throw<TException>(string because) where TException : Exception
    {
        try
        {
            return Throw<TException>();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<ActionAssertions> NotThrow(string because)
    {
        try
        {
            return NotThrow();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
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

    public ExceptionAssertions<TException> Throw<TException>(string because) where TException : Exception
    {
        try
        {
            return Throw<TException>();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<FunctionAssertions<T>> NotThrow(string because)
    {
        try
        {
            return NotThrow();
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
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
