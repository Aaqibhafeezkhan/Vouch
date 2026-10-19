namespace Aver;

internal static class Reason
{
    public static AverException Add(AverException failure, string? because)
    {
        if (string.IsNullOrWhiteSpace(because))
        {
            return failure;
        }

        var reason = because.Trim();
        if (!reason.EndsWith('.'))
        {
            reason += ".";
        }

        var separator = failure.Message.Contains('\n') ? "\n" : " ";
        return new AverException($"{failure.Message}{separator}Because {reason}");
    }
}
