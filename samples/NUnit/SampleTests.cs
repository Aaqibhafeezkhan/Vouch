using Aver;
using NUnit.Framework;

namespace Samples;

public class SampleTests
{
    [Test]
    public void Passes()
    {
        decimal total = 120m;
        total.Should().Be(120m);
    }

    [Test]
    public void Fails_on_purpose()
    {
        decimal total = 100m;
        total.Should().Be(120m);
    }
}
