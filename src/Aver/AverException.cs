using Aver.Sdk;

namespace Aver
{
    public sealed class AverException : Exception, IAssertionException
    {
        public AverException(string message) : base(message)
        {
        }
    }
}

namespace Aver.Sdk
{
    internal interface IAssertionException
    {
    }
}
