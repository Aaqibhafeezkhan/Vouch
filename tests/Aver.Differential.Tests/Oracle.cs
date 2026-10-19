using Xunit.Sdk;

namespace Differential;

public static class Oracle
{
    public static bool AverPasses(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (global::Aver.AverException)
        {
            return false;
        }
    }

    public static bool AwesomePasses(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (XunitException)
        {
            return false;
        }
    }

    public static string? AverMessage(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (global::Aver.AverException ex)
        {
            return ex.Message;
        }
    }

    public static string? AwesomeMessage(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (XunitException ex)
        {
            return ex.Message;
        }
    }

    public static string AverOutcome(Action action) => Outcome(action, typeof(global::Aver.AverException));

    public static string AwesomeOutcome(Action action) => Outcome(action, typeof(XunitException));

    private static string Outcome(Action action, Type assertionException)
    {
        try
        {
            action();
            return "passes";
        }
        catch (Exception ex) when (assertionException.IsInstanceOfType(ex))
        {
            return "fails";
        }
        catch (Exception ex)
        {
            return "throws " + ex.GetType().Name;
        }
    }
}
