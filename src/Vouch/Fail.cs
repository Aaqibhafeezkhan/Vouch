using System.Diagnostics.CodeAnalysis;

namespace Vouch;

internal static class Fail
{
    [DoesNotReturn]
    public static void With(string message) => throw new VouchException(message);

    public static string Name(string? expression) =>
        string.IsNullOrEmpty(expression) ? "value" : expression;
}
