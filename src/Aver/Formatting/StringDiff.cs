namespace Aver;

internal static class StringDiff
{
    private const int SnippetLength = 20;

    public static int FirstDifference(string actual, string expected)
    {
        var shared = Math.Min(actual.Length, expected.Length);
        for (var i = 0; i < shared; i++)
        {
            if (actual[i] != expected[i])
            {
                return i;
            }
        }

        return actual.Length == expected.Length ? -1 : shared;
    }

    public static string Describe(string actual, string expected)
    {
        var index = FirstDifference(actual, expected);
        if (index < 0)
        {
            return string.Empty;
        }

        var prefix = $"They differ at index {index}: ";
        if (index == expected.Length)
        {
            return prefix + $"expected the string to end there, found {Snippet(actual, index)}.";
        }

        if (index == actual.Length)
        {
            return prefix + $"expected {Snippet(expected, index)}, found the end of the string.";
        }

        return prefix + $"{Snippet(expected, index)} against {Snippet(actual, index)}.";
    }

    private static string Snippet(string text, int index) =>
        Formatter.Format(text.Substring(index, Math.Min(SnippetLength, text.Length - index)));
}
