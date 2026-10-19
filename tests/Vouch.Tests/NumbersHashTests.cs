using System.Diagnostics;
using Xunit;

namespace Vouch.Tests;

public class NumbersHashTests
{
    private static readonly object[] Values =
    {
        0.1f, 0.1m, 0.1d, 9007199254740993L, 9007199254740993m, 9007199254740992d, decimal.MaxValue,
        (double)decimal.MaxValue, float.NaN, double.NaN, 1, 1L, 1.0, 1.0f, 1m, ulong.MaxValue,
        1.8446744073709552E19, long.MaxValue, 9.2233720368547758E18d, double.PositiveInfinity,
        (byte)1, 123456792f, 123456792, 0.5f, 0.5m, 0.5d, -1, -1m, -1.0d, 0.0, -0.0, 0.0f, -0.0f, 0m,
        19.99m, 19.99d, 19.99f, 1234567.891m, 1234567.891d, 1e20m, 1e20d, 1e20f, -123456789012m, -123456789012L,
        float.MaxValue, (double)float.MaxValue, 0.3m, 0.1d + 0.2d,
    };

    [Fact]
    public void Equal_numbers_hash_alike()
    {
        foreach (var a in Values)
        {
            foreach (var b in Values)
            {
                if (Numbers.Equal(a, b))
                {
                    Assert.True(Numbers.Hash(a) == Numbers.Hash(b), $"{a.GetType().Name} {a} equals {b.GetType().Name} {b} but hashes differ");
                }
            }
        }
    }

    [Fact]
    public void Fractional_values_spread_over_many_hashes()
    {
        var hashes = new HashSet<int>();
        for (var i = 0; i < 1000; i++)
        {
            hashes.Add(Numbers.Hash(i * 0.01m + 0.005m));
        }

        Assert.True(hashes.Count > 900, $"only {hashes.Count} distinct hashes");
    }

    [Fact]
    public void Ten_thousand_fractional_decimals_match_in_reverse_quickly()
    {
        var subject = Enumerable.Range(0, 10_000).Select(i => (object)(i * 0.01m + 0.005m)).ToArray();
        var expected = subject.Reverse().ToArray();
        var clock = Stopwatch.StartNew();
        subject.Should().BeEquivalentTo(expected);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"took {clock.Elapsed}");
    }

    [Fact]
    public void Ten_thousand_whole_decimals_match_longs_in_reverse_quickly()
    {
        var subject = Enumerable.Range(0, 10_000).Select(i => (object)(1_000_000m + i)).ToArray();
        var expected = Enumerable.Range(0, 10_000).Select(i => (object)(1_000_000L + i)).Reverse().ToArray();
        var clock = Stopwatch.StartNew();
        subject.Should().BeEquivalentTo(expected);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"took {clock.Elapsed}");
    }

    [Fact]
    public void Ten_thousand_millisecond_timestamps_match_in_reverse_quickly()
    {
        var subject = Enumerable.Range(0, 10_000).Select(i => 1_700_000_000_000L + i).ToArray();
        var clock = Stopwatch.StartNew();
        subject.Should().BeEquivalentTo(subject.Reverse().ToArray());
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"took {clock.Elapsed}");
    }

    [Fact]
    public void Ten_thousand_large_decimals_match_in_reverse_quickly()
    {
        var subject = Enumerable.Range(0, 10_000).Select(i => 10_000_000_000m + i).ToArray();
        var clock = Stopwatch.StartNew();
        subject.Should().BeEquivalentTo(subject.Reverse().ToArray());
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"took {clock.Elapsed}");
    }

    [Fact]
    public void Mixed_float_and_double_lists_still_match_in_any_order()
    {
        var subject = Enumerable.Range(0, 40).Select(i => (object)(i + 0.5f)).ToArray();
        var expected = Enumerable.Range(0, 40).Select(i => (object)(i + 0.5d)).Reverse().ToArray();
        subject.Should().BeEquivalentTo(expected);
    }
}
