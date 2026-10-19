namespace Aver;

public readonly struct AsyncFunctionAssertions
{
    private readonly string? _expression;

    internal AsyncFunctionAssertions(Func<Task>? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Func<Task>? Subject { get; }

    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>() where TException : Exception
    {
        var name = Fail.Name(_expression);
        var expectation = $"to throw {Formatter.Format(typeof(TException))}";
        var thrown = await RunAsync(name, expectation).ConfigureAwait(false);
        return ExceptionMessages.Check<TException>(name, thrown);
    }

    public async Task NotThrowAsync()
    {
        var name = Fail.Name(_expression);
        var thrown = await RunAsync(name, "not to throw").ConfigureAwait(false);
        if (thrown is not null)
        {
            ExceptionMessages.Threw(name, thrown);
        }
    }

    private async Task<Exception?> RunAsync(string name, string expectation)
    {
        if (Subject is null)
        {
            ExceptionMessages.NullDelegate(name, expectation);
        }

        Task? task = null;
        Exception? thrown = null;
        try
        {
            task = Subject!();
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        if (thrown is null)
        {
            if (task is null)
            {
                ExceptionMessages.NullTask(name, expectation);
            }

            try
            {
                await task!.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
        }

        return thrown;
    }

    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because) where TException : Exception
    {
        try
        {
            return await ThrowAsync<TException>().ConfigureAwait(false);
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public async Task NotThrowAsync(string because)
    {
        try
        {
            await NotThrowAsync().ConfigureAwait(false);
        }
        catch (AverException failure)
        {
            throw Reason.Add(failure, because);
        }
    }
}
