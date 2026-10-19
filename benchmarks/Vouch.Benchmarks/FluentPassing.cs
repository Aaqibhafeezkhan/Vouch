using FluentAssertions;

namespace Benchmarks;

public static class FluentPassing
{
    public static void IntBe(int actual, int expected) => actual.Should().Be(expected);

    public static void StringBe(string actual, string expected) => actual.Should().Be(expected);

    public static void HaveCount(List<int> items, int count) => items.Should().HaveCount(count);

    public static void Contain(List<int> items, int item) => items.Should().Contain(item);

    public static async Task ThrowWithMessage(Func<Task> act, string pattern) =>
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(pattern);
}
