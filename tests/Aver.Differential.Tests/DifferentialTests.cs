using Xunit;
using Xunit.Abstractions;

namespace Differential;

public class DifferentialTests
{
    private readonly ITestOutputHelper _output;

    public DifferentialTests(ITestOutputHelper output) => _output = output;

    public static TheoryData<string> Names()
    {
        var data = new TheoryData<string>();
        foreach (var name in AwesomeCases.Pass.Keys)
        {
            data.Add(name);
        }

        return data;
    }

    public static TheoryData<string> MessageNames()
    {
        var data = new TheoryData<string>();
        foreach (var name in AwesomeCases.Messages.Keys)
        {
            data.Add(name);
        }

        return data;
    }

    private static readonly Dictionary<string, (string Awesome, string Aver, string Why)> Divergences = new()
    {
        ["match on null subject"] = ("throws NullReferenceException", "fails",
            "AwesomeAssertions crashes inside Match<T> on a null subject; both reject it, Aver says why"),
        ["match on wrong type"] = ("throws InvalidCastException", "fails",
            "AwesomeAssertions crashes inside Match<T> on another type; both reject it, Aver says why"),
    };

    [Fact]
    public void BothLibrariesDefineTheSameCases()
    {
        Assert.Equal(AwesomeCases.Pass.Keys.OrderBy(k => k), AverCases.Pass.Keys.OrderBy(k => k));
        Assert.Equal(AwesomeCases.Messages.Keys.OrderBy(k => k), AverCases.Messages.Keys.OrderBy(k => k));
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void AverAgreesWithAwesomeAssertions(string name)
    {
        var oracle = Oracle.AwesomeOutcome(AwesomeCases.Pass[name]);
        var aver = Oracle.AverOutcome(AverCases.Pass[name]);

        if (Divergences.TryGetValue(name, out var known))
        {
            Assert.True(
                (oracle, aver) == (known.Awesome, known.Aver),
                $"{name}: pinned divergence changed ({known.Why}). AwesomeAssertions {oracle}, Aver {aver}");
            return;
        }

        Assert.True(oracle == aver, $"{name}: AwesomeAssertions {oracle}, Aver {aver}");
    }

    [Fact]
    public void PrintsEveryOutcome()
    {
        foreach (var name in AwesomeCases.Pass.Keys.OrderBy(k => k))
        {
            _output.WriteLine($"{name} | awesome {Oracle.AwesomeOutcome(AwesomeCases.Pass[name])} | aver {Oracle.AverOutcome(AverCases.Pass[name])}");
        }
    }

    [Theory]
    [MemberData(nameof(MessageNames))]
    public void PrintsBothMessages(string name)
    {
        _output.WriteLine($"[{name}]");
        _output.WriteLine("Awesome: " + (Oracle.AwesomeMessage(AwesomeCases.Messages[name]) ?? "(passed)"));
        _output.WriteLine("Aver:    " + (Oracle.AverMessage(AverCases.Messages[name]) ?? "(passed)"));
    }
}
