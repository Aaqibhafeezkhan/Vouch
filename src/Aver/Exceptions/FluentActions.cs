namespace Aver;

public static class FluentActions
{
    public static Action Invoking(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return action;
    }

    public static Func<T> Invoking<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return func;
    }

    public static Func<Task> Awaiting(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return action;
    }

    public static Func<Task<T>> Awaiting<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return func;
    }
}
