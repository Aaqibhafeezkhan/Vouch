using Vouch.Sdk;

namespace Vouch;

public sealed class VouchException : Exception, IAssertionException
{
    public VouchException(string message) : base(message)
    {
    }
}
