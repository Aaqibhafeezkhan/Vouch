namespace Vouch;

internal static class Reason
{
    public static VouchException Add(VouchException failure, string? because)
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
        return new VouchException($"{failure.Message}{separator}Because {reason}");
    }
}
