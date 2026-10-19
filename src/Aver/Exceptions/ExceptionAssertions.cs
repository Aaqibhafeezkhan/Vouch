namespace Aver;

public sealed class ExceptionAssertions<TException> where TException : Exception
{
    private readonly string _name;

    internal ExceptionAssertions(TException exception, string name)
    {
        Which = exception;
        _name = name;
    }

    internal string Name => _name;

    public TException Which { get; }

    public TException Subject => Which;

    public ExceptionAssertions<TException> And => this;

    public ExceptionAssertions<TException> WithMessage(string pattern)
    {
        Wildcard.RequirePattern(pattern, nameof(pattern));
        if (!Wildcard.MatchesMessage(Which.Message, pattern))
        {
            Fail.With($"Expected {_name}'s exception message to match {Formatter.Format(pattern)}, but found {Formatter.Format(Which.Message)}.");
        }

        return this;
    }
}
