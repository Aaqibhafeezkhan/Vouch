using Xunit.Sdk;

namespace Differential;

public static class Oracle
{
    public static bool VouchPasses(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (global::Vouch.VouchException)
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

    public static string? VouchMessage(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (global::Vouch.VouchException ex)
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

    public static string VouchOutcome(Action action) => Outcome(action, typeof(global::Vouch.VouchException));

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
