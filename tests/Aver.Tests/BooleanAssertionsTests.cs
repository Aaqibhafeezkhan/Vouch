using Xunit;

namespace Aver.Tests;

public class BooleanAssertionsTests
{
    [Fact] public void BeTrue_passes() => true.Should().BeTrue();

    [Fact]
    public void BeTrue_fails_with_exact_message()
    {
        var ready = false;
        Assert.Equal("Expected ready to be true, but found false.", Assert.Throws<AverException>(() => ready.Should().BeTrue()).Message);
    }

    [Fact] public void BeFalse_passes() => false.Should().BeFalse();

    [Fact]
    public void BeFalse_fails_with_exact_message()
    {
        var ready = true;
        Assert.Equal("Expected ready to be false, but found true.", Assert.Throws<AverException>(() => ready.Should().BeFalse()).Message);
    }

    [Fact] public void Be_passes() => true.Should().Be(true);

    [Fact]
    public void Be_fails_with_exact_message()
    {
        var ready = true;
        Assert.Equal("Expected ready to be false, but found true.", Assert.Throws<AverException>(() => ready.Should().Be(false)).Message);
    }

    [Fact] public void NotBe_passes() => true.Should().NotBe(false);

    [Fact]
    public void NotBe_fails_with_exact_message()
    {
        var ready = true;
        Assert.Equal("Expected ready not to be true, but it was.", Assert.Throws<AverException>(() => ready.Should().NotBe(true)).Message);
    }

    [Fact] public void Bool_goes_to_BooleanAssertions() => Assert.IsType<BooleanAssertions>(true.Should());

    [Fact] public void Nullable_bool_goes_to_NullableBooleanAssertions() => Assert.IsType<NullableBooleanAssertions>(((bool?)true).Should());

    [Fact] public void Nullable_BeTrue_passes() { bool? b = true; b.Should().BeTrue(); }

    [Fact]
    public void Nullable_BeTrue_fails_on_null_with_exact_message()
    {
        bool? b = null;
        Assert.Equal("Expected b to be true, but found null.", Assert.Throws<AverException>(() => b.Should().BeTrue()).Message);
    }

    [Fact] public void Nullable_BeFalse_passes() { bool? b = false; b.Should().BeFalse(); }

    [Fact] public void BeFalse_on_null_bool_fails() { bool? b = null; Assert.Throws<AverException>(() => b.Should().BeFalse()); }

    [Fact]
    public void Nullable_BeFalse_fails_on_true_with_exact_message()
    {
        bool? b = true;
        Assert.Equal("Expected b to be false, but found true.", Assert.Throws<AverException>(() => b.Should().BeFalse()).Message);
    }

    [Fact] public void Nullable_Be_passes() { bool? b = true; b.Should().Be(true); }

    [Fact]
    public void Nullable_Be_fails_with_exact_message()
    {
        bool? b = null;
        Assert.Equal("Expected b to be true, but found null.", Assert.Throws<AverException>(() => b.Should().Be(true)).Message);
    }

    [Fact] public void Nullable_NotBe_passes_on_null() { bool? b = null; b.Should().NotBe(true); }

    [Fact]
    public void Nullable_NotBe_fails_with_exact_message()
    {
        bool? b = true;
        Assert.Equal("Expected b not to be true, but it was.", Assert.Throws<AverException>(() => b.Should().NotBe(true)).Message);
    }

    [Fact] public void Nullable_BeNull_passes() { bool? b = null; b.Should().BeNull(); }

    [Fact]
    public void Nullable_BeNull_fails_with_exact_message()
    {
        bool? b = false;
        Assert.Equal("Expected b to be null, but found false.", Assert.Throws<AverException>(() => b.Should().BeNull()).Message);
    }

    [Fact] public void Nullable_NotBeNull_Which_is_the_value() { bool? b = false; Assert.False(b.Should().NotBeNull().Which); }

    [Fact]
    public void Nullable_NotBeNull_fails_with_exact_message()
    {
        bool? b = null;
        Assert.Equal("Expected b not to be null, but it was.", Assert.Throws<AverException>(() => b.Should().NotBeNull()).Message);
    }
}
