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
        Assert.Equal("Expected order to be Order { Total = 120 }, but found Order { Total = 100 }.", ex.Message);
    }

    [Fact] public void NotBe_passes_on_different_objects() => new Order(1).Should().NotBe(new Order(2));

    [Fact]
    public void NotBe_fails_on_equal_objects_with_exact_message()
    {
        var order = new Order(100);
        var ex = Assert.Throws<AverException>(() => order.Should().NotBe(new Order(100)));
        Assert.Equal("Expected order not to be Order { Total = 100 }, but it was.", ex.Message);
    }

    [Fact] public void BeNull_passes_on_null() { object? o = null; o.Should().BeNull(); }

    [Fact]
    public void BeNull_fails_naming_the_expression()
    {
        object thing = "x";
        var ex = Assert.Throws<AverException>(() => thing.Should().BeNull());
        Assert.Equal("Expected thing to be null, but found \"x\".", ex.Message);
    }

    [Fact] public void NotBeNull_passes_on_a_value() { object o = 1; o.Should().NotBeNull(); }

    [Fact]
    public void NotBeNull_fails_on_null_with_exact_message()
    {
        object? o = null;
        var ex = Assert.Throws<AverException>(() => o.Should().NotBeNull());
        Assert.Equal("Expected o not to be null, but it was.", ex.Message);
    }

    [Fact] public void And_chains_back_to_the_same_assertions() { object o = 1; o.Should().NotBeNull().And.Be(1); }

    [Fact] public void Missing_expression_reads_as_value() => Assert.Equal("value", Fail.Name(null));

    private static string Msg(Action act) => Assert.Throws<AverException>(act).Message;

    [Fact] public void Match_passes_when_the_predicate_holds() { object o = new Order(5); o.Should().Match<Order>(x => x.Total == 5); }
    [Fact] public void Match_works_on_a_typed_subject() { var order = new Order(5); order.Should().Match<Order>(x => x.Total > 1); }
    [Fact] public void Match_returns_the_assertions_for_chaining() { object o = new Order(5); o.Should().Match<Order>(x => x.Total == 5).And.NotBeNull(); }
    [Fact] public void Match_fails_when_the_predicate_does_not_hold() { object o = new Order(5); Assert.Throws<AverException>(() => o.Should().Match<Order>(x => x.Total == 6)); }
    [Fact] public void Match_message_names_the_predicate_and_the_value() { var o = new Order(5); Assert.Equal("Expected o to match (x.Total == 6), but found Order { Total = 5 }.", Msg(() => o.Should().Match<Order>(x => x.Total == 6))); }
    [Fact] public void Match_fails_on_null() { object? o = null; Assert.Equal("Expected o to match (x.Total == 5), but found null.", Msg(() => o.Should().Match<Order>(x => x.Total == 5))); }
    [Fact] public void Match_fails_on_another_type() { object o = "text"; Assert.Equal("Expected o to be of type Order matching (x.Total == 5), but found String.", Msg(() => o.Should().Match<Order>(x => x.Total == 5))); }
    [Fact] public void Match_null_predicate_throws_ArgumentNullException() { object o = new Order(5); Assert.Throws<ArgumentNullException>(() => o.Should().Match<Order>(null!)); }
    [Fact] public void Match_takes_a_reason() { var o = new Order(5); o.Should().Match<Order>(x => x.Total == 5, "it is five"); Assert.Equal("Expected o to match (x.Total == 6), but found Order { Total = 5 }. Because it is six.", Msg(() => o.Should().Match<Order>(x => x.Total == 6, "it is six"))); }

    [Fact] public void BeOfType_by_Type_passes_on_the_exact_type() { object o = new Order(1); o.Should().BeOfType(typeof(Order)); }
    [Fact] public void BeOfType_by_Type_is_exact() { object o = new Order(1); Assert.Throws<AverException>(() => o.Should().BeOfType(typeof(object))); }
    [Fact] public void BeOfType_by_Type_message() { object o = new Order(1); Assert.Equal("Expected o to be of type String, but found Order.", Msg(() => o.Should().BeOfType(typeof(string)))); }
    [Fact] public void BeOfType_by_Type_fails_on_null() { object? o = null; Assert.Equal("Expected o to be of type String, but found null.", Msg(() => o.Should().BeOfType(typeof(string)))); }
    [Fact] public void BeOfType_by_Type_null_type_throws_ArgumentNullException() { object o = new Order(1); Assert.Throws<ArgumentNullException>(() => o.Should().BeOfType((Type)null!)); }

    [Fact] public void Be_treats_numbers_of_different_types_with_the_same_value_as_equal() { object o = 1; o.Should().Be(1L); }
    [Fact] public void Be_compares_integers_and_floating_point_by_value() { object o = 1; o.Should().Be(1.0); }
    [Fact] public void Be_compares_decimal_and_double_by_value() { object o = 0.1m; o.Should().Be(0.1); }
    [Fact] public void Be_fails_on_numbers_with_different_values() { object o = 1; Assert.Throws<AverException>(() => o.Should().Be(2L)); }
    [Fact] public void Be_fails_on_a_fractional_value() { object o = 2; Assert.Throws<AverException>(() => o.Should().Be(1.5)); }
    [Fact] public void Be_does_not_widen_a_float_to_a_double() { object o = 0.1f; Assert.Throws<AverException>(() => o.Should().Be(0.1)); }
    [Fact] public void Be_does_not_compare_a_signed_negative_to_an_unsigned() { object o = -1; Assert.Throws<AverException>(() => o.Should().Be(ulong.MaxValue)); }
    [Fact] public void Be_does_not_compare_a_number_to_a_char_or_a_bool() { object o = 97; Assert.Throws<AverException>(() => o.Should().Be('a')); Assert.Throws<AverException>(() => o.Should().Be(true)); }
    [Fact] public void Be_does_not_compare_a_number_to_an_enum() { object o = 1; Assert.Throws<AverException>(() => o.Should().Be(DayOfWeek.Monday)); }
    [Fact] public void Be_message_for_a_number_of_another_type() { object o = 1; Assert.Equal("Expected o to be 2L, but found 1.", Msg(() => o.Should().Be(2L))); }
    [Fact] public void NotBe_fails_on_the_same_value_in_another_numeric_type() { object o = 1; Assert.Throws<AverException>(() => o.Should().NotBe(1L)); }
    [Fact]
    public void BeOneOf_does_not_match_numbers_across_types()
    {
        object o = 1;
        Assert.Throws<AverException>(() => o.Should().BeOneOf(5L, 1L));
        Assert.Throws<AverException>(() => o.Should().BeOneOf(1.0));
        object b = (byte)5;
        Assert.Throws<AverException>(() => b.Should().BeOneOf((sbyte)5));
        object f = 0.5f;
        Assert.Throws<AverException>(() => f.Should().BeOneOf(0.5d));
        object m = 0.1m;
        Assert.Throws<AverException>(() => m.Should().BeOneOf(0.1d));
    }
    [Fact] public void BeOneOf_matches_the_same_type() { object o = 1; o.Should().BeOneOf(5, 1); }
    [Theory]
    [InlineData(9007199254740993L, 9007199254740992d)]
    [InlineData(9007199254740992L, 9007199254740992d)]
    [InlineData(9007199254740990L, 9007199254740992d)]
    public void Be_does_not_equate_a_decimal_with_a_double_that_rounds_to_it(long whole, double other)
    {
        object d = (decimal)whole;
        Assert.Throws<AverException>(() => d.Should().Be(other));
        object o = other;
        Assert.Throws<AverException>(() => o.Should().Be((decimal)whole));
        d.Should().NotBe(other);
    }
    [Fact]
    public void Be_does_not_equate_decimal_max_with_its_double()
    {
        object d = decimal.MaxValue;
        Assert.Throws<AverException>(() => d.Should().Be((double)decimal.MaxValue));
        d.Should().NotBe((double)decimal.MaxValue);
        object big = 79228162514264337593543950335m;
        Assert.Throws<AverException>(() => big.Should().Be(7.9228162514264338E28d));
    }
    [Fact] public void Be_equates_a_float_with_the_decimal_it_prints_as() { object f = 0.1f; f.Should().Be(0.1m); object m = 0.1m; m.Should().Be(0.1f); }
    [Fact] public void Be_equates_a_float_nan_with_a_double_nan() { object f = float.NaN; f.Should().Be(double.NaN); f.Should().NotBe(1.0); Assert.Throws<AverException>(() => f.Should().NotBe(double.NaN)); }
    [Fact] public void Be_does_not_equate_a_decimal_with_nan_or_infinity() { object m = 1m; Assert.Throws<AverException>(() => m.Should().Be(double.NaN)); Assert.Throws<AverException>(() => m.Should().Be(double.PositiveInfinity)); }
    [Fact] public void Be_equates_a_float_with_a_whole_int_beyond_seven_digits() { object f = 123456792f; f.Should().Be(123456792); }
}
