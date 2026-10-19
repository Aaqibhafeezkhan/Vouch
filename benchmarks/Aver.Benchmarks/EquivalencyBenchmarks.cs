using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class EquivalencyBenchmarks
{
    private readonly Order _order = Fixtures.MakeOrder(7);
    private readonly Order _sameOrder = Fixtures.MakeOrder(7);
    private readonly List<Order> _orders = Fixtures.MakeOrders(100);
    private readonly List<Order> _reversedOrders = Fixtures.MakeOrders(100).AsEnumerable().Reverse().ToList();
    private readonly List<Row> _rows = Fixtures.MakeRows(1000);
    private readonly List<Row> _reversedRows = Fixtures.MakeRows(1000).AsEnumerable().Reverse().ToList();

    [Benchmark] public void Aver_OneRecord() => AverEquivalency.One(_order, _sameOrder);
    [Benchmark] public void Fluent_OneRecord() => FluentEquivalency.One(_order, _sameOrder);

    [Benchmark] public void Aver_HundredRecordsReordered() => AverEquivalency.Orders(_reversedOrders, _orders);
    [Benchmark] public void Fluent_HundredRecordsReordered() => FluentEquivalency.Orders(_reversedOrders, _orders);

    [Benchmark] public void Aver_ThousandRowsReordered() => AverEquivalency.Rows(_reversedRows, _rows);
    [Benchmark] public void Fluent_ThousandRowsReordered() => FluentEquivalency.Rows(_reversedRows, _rows);
}
