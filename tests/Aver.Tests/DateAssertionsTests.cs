using Xunit;

namespace Aver.Tests;

public class DateAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<AverException>(act).Message;

    private static readonly DateTime Now = new(2026, 10, 19, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Yesterday = Now.AddDays(-1);

    [Fact] public void DateTime_Be_passes() { Now.Should().Be(Now); }
    [Fact] public void DateTime_Be_message() { var at = Yesterday; Assert.Equal("Expected at to be 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().Be(Now))); }
    [Fact] public void DateTime_NotBe_passes() { Now.Should().NotBe(Yesterday); }
    [Fact] public void DateTime_NotBe_message() { var at = Now; Assert.Equal("Expected at not to be 2026-10-19T08:30:00.0000000Z, but it was.", Message(() => at.Should().NotBe(Now))); }
    [Fact] public void DateTime_BeAfter_passes() { Now.Should().BeAfter(Yesterday); }
    [Fact] public void DateTime_BeAfter_fails_on_equal() { Assert.Throws<AverException>(() => Now.Should().BeAfter(Now)); }
    [Fact] public void DateTime_BeAfter_message() { var at = Yesterday; Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().BeAfter(Now))); }
    [Fact] public void DateTime_BeBefore_passes() { Yesterday.Should().BeBefore(Now); }
    [Fact] public void DateTime_BeBefore_message() { var at = Now; Assert.Equal("Expected at to be before 2026-10-18T08:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().BeBefore(Yesterday))); }
    [Fact] public void DateTime_BeOnOrAfter_passes() { Now.Should().BeOnOrAfter(Now); }
    [Fact] public void DateTime_BeOnOrAfter_message() { var at = Yesterday; Assert.Equal("Expected at to be on or after 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().BeOnOrAfter(Now))); }
    [Fact] public void DateTime_BeOnOrBefore_passes() { Now.Should().BeOnOrBefore(Now); }
    [Fact] public void DateTime_BeOnOrBefore_message() { var at = Now; Assert.Equal("Expected at to be on or before 2026-10-18T08:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().BeOnOrBefore(Yesterday))); }
    [Fact] public void DateTime_BeCloseTo_passes_on_boundary() { Now.AddSeconds(5).Should().BeCloseTo(Now, TimeSpan.FromSeconds(5)); }
    [Fact] public void DateTime_BeCloseTo_passes_before() { Now.AddSeconds(-5).Should().BeCloseTo(Now, TimeSpan.FromSeconds(5)); }
    [Fact] public void DateTime_BeCloseTo_message() { var at = Now.AddMinutes(1); Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:31:00.0000000Z (off by 00:01:00).", Message(() => at.Should().BeCloseTo(Now, TimeSpan.FromSeconds(5)))); }

    [Fact]
    public void Nullable_DateTime_null_fails()
    {
        DateTime? at = null;
        Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeAfter(Now)));
        Assert.Equal("Expected at to be before 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeBefore(Now)));
        Assert.Equal("Expected at to be on or after 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeOnOrAfter(Now)));
        Assert.Equal("Expected at to be on or before 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeOnOrBefore(Now)));
        Assert.Equal("Expected at to be 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().Be(Now)));
        Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeCloseTo(Now, TimeSpan.FromSeconds(5))));
        at.Should().NotBe(Now);
        at.Should().BeNull();
    }
    [Fact]
    public void Nullable_DateTime_passes()
    {
        DateTime? at = Now;
        at.Should().Be(Now).And.BeOnOrAfter(Now).And.BeOnOrBefore(Now).And.BeAfter(Yesterday).And.BeBefore(Now.AddDays(1));
        at.Should().BeCloseTo(Now, TimeSpan.Zero);
        Assert.Equal(Now, at.Should().NotBeNull().Which);
    }
    [Fact]
    public void Nullable_DateTime_fails_with_value()
    {
        DateTime? at = Now.AddMinutes(1);
        Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:31:00.0000000Z (off by 00:01:00).", Message(() => at.Should().BeCloseTo(Now, TimeSpan.FromSeconds(5))));
        Assert.Equal("Expected at to be null, but found 2026-10-19T08:31:00.0000000Z.", Message(() => at.Should().BeNull()));
        Assert.Equal("Expected at not to be 2026-10-19T08:31:00.0000000Z, but it was.", Message(() => at.Should().NotBe(Now.AddMinutes(1))));
        DateTime? none = null;
        Assert.Equal("Expected none not to be null, but it was.", Message(() => none.Should().NotBeNull()));
    }

    private static readonly DateTimeOffset Offset = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    [Fact] public void DateTimeOffset_Be_compares_the_instant() { Offset.Should().Be(new DateTimeOffset(2026, 10, 19, 16, 30, 0, TimeSpan.FromHours(8))); }
    [Fact] public void DateTimeOffset_Be_message() { var at = Offset.AddDays(-1); Assert.Equal("Expected at to be 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-18T08:30:00.0000000+00:00.", Message(() => at.Should().Be(Offset))); }
    [Fact] public void DateTimeOffset_NotBe() { Offset.Should().NotBe(Offset.AddHours(1)); Assert.Throws<AverException>(() => Offset.Should().NotBe(Offset)); }
    [Fact]
    public void DateTimeOffset_ordering()
    {
        Offset.Should().BeAfter(Offset.AddSeconds(-1)).And.BeBefore(Offset.AddSeconds(1)).And.BeOnOrAfter(Offset).And.BeOnOrBefore(Offset);
        Assert.Throws<AverException>(() => Offset.Should().BeAfter(Offset));
        Assert.Throws<AverException>(() => Offset.Should().BeBefore(Offset));
        Assert.Throws<AverException>(() => Offset.Should().BeOnOrAfter(Offset.AddSeconds(1)));
        Assert.Throws<AverException>(() => Offset.Should().BeOnOrBefore(Offset.AddSeconds(-1)));
    }
    [Fact] public void DateTimeOffset_BeAfter_message() { var at = Offset.AddDays(-1); Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-18T08:30:00.0000000+00:00.", Message(() => at.Should().BeAfter(Offset))); }
    [Fact] public void DateTimeOffset_BeCloseTo() { Offset.AddSeconds(5).Should().BeCloseTo(Offset, TimeSpan.FromSeconds(5)); var at = Offset.AddMinutes(1); Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-19T08:31:00.0000000+00:00 (off by 00:01:00).", Message(() => at.Should().BeCloseTo(Offset, TimeSpan.FromSeconds(5)))); }
    [Fact]
    public void Nullable_DateTimeOffset()
    {
        DateTimeOffset? at = null;
        at.Should().BeNull();
        Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000+00:00, but found null.", Message(() => at.Should().BeAfter(Offset)));
        DateTimeOffset? some = Offset;
        some.Should().Be(Offset).And.BeCloseTo(Offset, TimeSpan.Zero);
        Assert.Equal(Offset, some.Should().NotBeNull().Which);
    }

    private static readonly DateOnly Day = new(2026, 10, 19);

    [Fact] public void DateOnly_Be_passes() { Day.Should().Be(Day); }
    [Fact] public void DateOnly_Be_message() { var d = Day.AddDays(-1); Assert.Equal("Expected d to be 2026-10-19, but found 2026-10-18.", Message(() => d.Should().Be(Day))); }
    [Fact] public void DateOnly_NotBe() { Day.Should().NotBe(Day.AddDays(1)); var d = Day; Assert.Equal("Expected d not to be 2026-10-19, but it was.", Message(() => d.Should().NotBe(Day))); }
    [Fact]
    public void DateOnly_ordering()
    {
        Day.Should().BeAfter(Day.AddDays(-1)).And.BeBefore(Day.AddDays(1)).And.BeOnOrAfter(Day).And.BeOnOrBefore(Day);
        var d = Day;
        Assert.Equal("Expected d to be after 2026-10-19, but found 2026-10-19.", Message(() => d.Should().BeAfter(Day)));
        Assert.Equal("Expected d to be before 2026-10-19, but found 2026-10-19.", Message(() => d.Should().BeBefore(Day)));
        Assert.Equal("Expected d to be on or after 2026-10-20, but found 2026-10-19.", Message(() => d.Should().BeOnOrAfter(Day.AddDays(1))));
        Assert.Equal("Expected d to be on or before 2026-10-18, but found 2026-10-19.", Message(() => d.Should().BeOnOrBefore(Day.AddDays(-1))));
    }
    [Fact]
    public void Nullable_DateOnly()
    {
        DateOnly? d = null;
        d.Should().BeNull();
        Assert.Equal("Expected d to be after 2026-10-19, but found null.", Message(() => d.Should().BeAfter(Day)));
        DateOnly? some = Day;
        some.Should().Be(Day).And.BeOnOrAfter(Day);
        Assert.Equal(Day, some.Should().NotBeNull().Which);
    }

    [Fact] public void TimeSpan_still_resolves() { TimeSpan.FromSeconds(1).Should().Be(TimeSpan.FromSeconds(1)); TimeSpan? t = null; t.Should().BeNull(); }
    [Fact]
    public void Overloads_resolve_to_the_specific_assertions()
    {
        Assert.IsType<NumericAssertions<int>>(1.Should());
        Assert.IsType<NumericAssertions<long>>(1L.Should());
        Assert.IsType<NumericAssertions<decimal>>(1m.Should());
        Assert.IsType<NumericAssertions<double>>(1d.Should());
        Assert.IsType<NullableNumericAssertions<int>>(((int?)1).Should());
        Assert.IsType<DateTimeAssertions>(Now.Should());
        Assert.IsType<DateTimeOffsetAssertions>(Offset.Should());
        Assert.IsType<DateOnlyAssertions>(Day.Should());
        Assert.IsType<NullableDateTimeAssertions>(((DateTime?)Now).Should());
        Assert.IsType<ValueAssertions<TimeSpan>>(TimeSpan.Zero.Should());
    }
}
