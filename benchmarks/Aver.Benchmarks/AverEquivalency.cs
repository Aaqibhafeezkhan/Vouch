using Aver;

namespace Benchmarks;

public static class AverEquivalency
{
    public static void One(Order actual, Order expected) => actual.Should().BeEquivalentTo(expected);

    public static void Orders(List<Order> actual, List<Order> expected) => actual.Should().BeEquivalentTo(expected);

    public static void Rows(List<Row> actual, List<Row> expected) => actual.Should().BeEquivalentTo(expected);
}
