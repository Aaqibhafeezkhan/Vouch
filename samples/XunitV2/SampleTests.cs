using Aver;
using Xunit;

namespace Samples;

public class SampleTests
{
    [Fact]
    public void Passes()
    {
        decimal total = 120m;
        total.Should().Be(120m);
    }

    [Fact]
    public void Fails_on_purpose()
    {
        decimal total = 100m;
        total.Should().Be(120m);
    }
}
