using Xunit;

namespace Aver.Tests;

public class AllocationTests
{
    private const int Calls = 1000;

    private static void AssertNoAllocation(Action call)
    {
        call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) call();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact] public void Int_Be() { var n = 5; AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Guid_Be() { var id = Guid.NewGuid(); AssertNoAllocation(() => id.Should().Be(id)); }

    [Fact] public void Enum_Be() { var s = Status.Active; AssertNoAllocation(() => s.Should().Be(Status.Active)); }

    [Fact] public void Bool_BeTrue() { var b = true; AssertNoAllocation(() => b.Should().BeTrue()); }

    [Fact] public void Nullable_int_Be() { int? n = 5; AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Decimal_Be() { var m = 100m; AssertNoAllocation(() => m.Should().Be(100m)); }

    [Fact] public void Int_BeGreaterThan() { var n = 5; AssertNoAllocation(() => n.Should().BeGreaterThan(3)); }

    [Fact] public void DateTime_BeAfter() { var now = DateTime.UtcNow; var past = now.AddDays(-1); AssertNoAllocation(() => now.Should().BeAfter(past)); }

    [Fact] public void String_Be() { var s = "abc"; AssertNoAllocation(() => s.Should().Be("abc")); }

    [Fact] public void String_StartWith() { var s = "abc"; AssertNoAllocation(() => s.Should().StartWith("ab")); }

    [Fact] public void String_Contain() { var s = "abc"; AssertNoAllocation(() => s.Should().Contain("b")); }

    [Fact] public void String_NotBeNullOrEmpty() { var s = "abc"; AssertNoAllocation(() => s.Should().NotBeNullOrEmpty()); }

    private static long BytesForContainKey(int size)
    {
        var d = Enumerable.Range(0, size).ToDictionary(i => i, i => i);
        d.Should().ContainKey(size - 1);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) d.Should().ContainKey(size - 1);
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void Dictionary_ContainKey_allocation_does_not_grow_with_size()
    {
        var small = BytesForContainKey(10);
        var large = BytesForContainKey(10_000);
        Assert.Equal(small, large);
        Assert.True(large / Calls <= 128);
    }
}
