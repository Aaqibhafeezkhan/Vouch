using Aver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Samples;

[TestClass]
public class SampleTests
{
    [TestMethod]
    public void Passes()
    {
        decimal total = 120m;
        total.Should().Be(120m);
    }

    [TestMethod]
    public void Fails_on_purpose()
    {
        decimal total = 100m;
        total.Should().Be(120m);
    }
}
