namespace Aver;

internal static class ExceptionMessages
{
    public static void ThrewWrongType(string name, Type expected, Exception actual) =>
        Fail.With($"Expected {name} to throw {Formatter.Format(expected)}, but it threw {Formatter.Format(actual.GetType())}:\n  {Formatter.Format(actual.Message)}");

    public static void DidNotThrow(string name, Type expected) =>
        Fail.With($"Expected {name} to throw {Formatter.Format(expected)}, but it did not throw.");

    public static void Threw(string name, Exception actual)
    {
        var message = $"Expected {name} not to throw, but it threw {Formatter.Format(actual.GetType())}:\n  {Formatter.Format(actual.Message)}";
        var stack = actual.StackTrace;
        if (!string.IsNullOrEmpty(stack))
        {
            var end = stack.IndexOf('\n');
            var first = (end < 0 ? stack : stack[..end]).Trim();
            if (first.Length > 0)
            {
                message += "\n  " + first;
            }
        }

        Fail.With(message);
    }

    public static void NullDelegate(string name, string expectation) =>
        Fail.With($"Expected {name} {expectation}, but found null.");

    public static void NullTask(string name, string expectation) =>
        Fail.With($"Expected {name} {expectation}, but it returned a null Task.");

    public static ExceptionAssertions<T> Check<T>(string name, Exception? thrown) where T : Exception
    {
        if (thrown is T match)
        {
            return new ExceptionAssertions<T>(match, name);
        }

        if (thrown is null)
        {
            DidNotThrow(name, typeof(T));
        }
        else
        {
            ThrewWrongType(name, typeof(T), thrown);
        }

        throw new InvalidOperationException("unreachable");
    }
}
