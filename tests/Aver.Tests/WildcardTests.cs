using System.Diagnostics;
using Xunit;

namespace Aver.Tests;

public class WildcardTests
{
    [Theory]
    [InlineData("INV-001", "INV-*", true)]
    [InlineData("INV-001", "INV-00?", true)]
    [InlineData("INV-001", "inv-*", false)]
    [InlineData("", "*", true)]
    [InlineData("abc", "a*c*", true)]
    [InlineData("a.c", "a.c", true)]
    [InlineData("abc", "a.c", false)]
    [InlineData("", "", true)]
    [InlineData("a", "", false)]
    [InlineData("", "?", false)]
    [InlineData("abc", "***", true)]
    [InlineData("abcb", "*b", true)]
    [InlineData("abc", "*b", false)]
    [InlineData("aXbXc", "a*b*c", true)]
    public void Wildcard_cases(string text, string pattern, bool expected) =>
        Assert.Equal(expected, Wildcard.IsMatch(text, pattern));

    [Fact]
    public void Wildcard_pathological_pattern_is_fast()
    {
        var sw = Stopwatch.StartNew();
        Assert.False(Wildcard.IsMatch(new string('a', 10_000), "*a*a*a*a*a*a*b"));
        Assert.True(sw.ElapsedMilliseconds < 200);
    }
}
