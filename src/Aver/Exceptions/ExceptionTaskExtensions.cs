namespace Aver;

public static class ExceptionTaskExtensions
{
    public static Task<ExceptionAssertions<T>> WithMessage<T>(this Task<ExceptionAssertions<T>> task, string pattern)
        where T : Exception
    {
        ArgumentNullException.ThrowIfNull(task);
        Wildcard.RequirePattern(pattern, nameof(pattern));
        return WithMessageCore(task, pattern);
    }

    private static async Task<ExceptionAssertions<T>> WithMessageCore<T>(Task<ExceptionAssertions<T>> task, string pattern)
        where T : Exception
    {
        var assertions = await task.ConfigureAwait(false);
        return assertions.WithMessage(pattern);
    }
}
