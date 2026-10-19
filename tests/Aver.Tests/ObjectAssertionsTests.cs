using Xunit;

namespace Aver.Tests;

public record Order(int Total);

public class ObjectAssertionsTests
{
    [Fact] public void Be_passes_on_equal_objects() => new Order(1).Should().Be(new Order(1));

    [Fact]
    public void Be_fails_with_expression_and_both_values()
    {
        var order = new Order(100);
        var ex = Assert.Throws<AverException>(() => order.Should().Be(new Order(120)));
        Assert.StartsWith("Expected order to be ", ex.Message);
        Assert.Contains("but found ", ex.Message);
    }

    [Fact(Skip = "needs the formatter")]
    public void BeNull_fails_naming_the_expression()
    {
        object thing = "x";
        var ex = Assert.Throws<AverException>(() => thing.Should().BeNull());
        Assert.Equal("Expected thing to be null, but found \"x\".", ex.Message);
    }

    [Fact] public void NotBeNull_fails_on_null() { object? o = null; Assert.Throws<AverException>(() => o.Should().NotBeNull()); }

    [Fact] public void And_chains_back_to_the_same_assertions() { object o = 1; o.Should().NotBeNull().And.Be(1); }

    [Fact] public void Missing_expression_reads_as_value() => Assert.Equal("value", Fail.Name(null));
}
