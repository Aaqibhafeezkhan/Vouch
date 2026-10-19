using Xunit;

namespace Aver.Tests;

public class StringDiffTests
{
    [Fact]
    public void Points_at_index()
        => Assert.Equal("They differ at index 10: \"Cruz\" against \"Kruz\".", StringDiff.Describe("Juan Dela Kruz", "Juan Dela Cruz"));

    [Fact]
    public void Reports_length_when_expected_is_a_prefix()
        => Assert.Equal("They differ at index 4: expected the string to end there, found \"-2\".", StringDiff.Describe("ABCD-2", "ABCD"));

    [Fact]
    public void Reports_length_when_actual_is_a_prefix()
        => Assert.Equal("They differ at index 4: expected \"-2\", found the end of the string.", StringDiff.Describe("ABCD", "ABCD-2"));

    [Fact]
    public void Equal_strings_give_minus_one_and_empty()
    {
        Assert.Equal(-1, StringDiff.FirstDifference("abc", "abc"));
        Assert.Equal("", StringDiff.Describe("abc", "abc"));
    }

    [Fact]
    public void Snippets_are_twenty_chars()
        => Assert.Equal("They differ at index 0: \"bbbbbbbbbbbbbbbbbbbb\" against \"aaaaaaaaaaaaaaaaaaaa\".", StringDiff.Describe(new string('a', 40), new string('b', 40)));
}
