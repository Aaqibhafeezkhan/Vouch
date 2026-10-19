namespace Vouch;

public static class InvokingExtensions
{
    public static Action Invoking<T>(this T subject, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return () => action(subject);
    }

    public static Func<TResult> Invoking<T, TResult>(this T subject, Func<T, TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return () => func(subject);
    }
}
