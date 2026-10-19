namespace Aver;

public static class ExceptionTaskExtensions
{
    public static async Task<ExceptionAssertions<T>> WithMessage<T>(this Task<ExceptionAssertions<T>> task, string pattern)
        where T : Exception
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(pattern);
        var assertions = await task.ConfigureAwait(false);
        return assertions.WithMessage(pattern);
    }
}
