using System.Runtime.CompilerServices;

namespace Aver;

public static class ShouldExtensions
{
    public static ObjectAssertions Should(this object? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static ValueAssertions<T> Should<T>(this T subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        where T : struct
        => new(subject, expression);

    public static NullableValueAssertions<T> Should<T>(this T? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        where T : struct
        => new(subject, expression);

    public static BooleanAssertions Should(this bool subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableBooleanAssertions Should(this bool? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);
}
