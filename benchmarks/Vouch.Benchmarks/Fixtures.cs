namespace Benchmarks;

public record Tag(string Label, int Weight);

public record Order(
    int Id,
    string Customer,
    decimal Total,
    DateTime Placed,
    bool Paid,
    List<Tag> Tags);

public record Row(long Id, string Name);

public static class Fixtures
{
    public static Order MakeOrder(int id) => new(
        id,
        "customer-" + id,
        19.99m + id,
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(id),
        id % 2 == 0,
        new List<Tag> { new("a" + id, 1), new("b" + id, 2), new("c" + id, 3) });

    public static List<Order> MakeOrders(int count) =>
        Enumerable.Range(0, count).Select(MakeOrder).ToList();

    public static List<Row> MakeRows(int count) =>
        Enumerable.Range(0, count).Select(i => new Row(1_700_000_000_000L + i, "same")).ToList();
}
