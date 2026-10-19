using System.Runtime.CompilerServices;

namespace Aver;

public static class ShouldExtensions
{
    public static ObjectAssertions Should(this object? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);
}
