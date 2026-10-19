using System.Diagnostics.CodeAnalysis;

namespace Aver;

internal static class Fail
{
    [DoesNotReturn]
    public static void With(string message) => throw new AverException(message);

    public static string Name(string? expression) =>
        string.IsNullOrEmpty(expression) ? "value" : expression;
}
