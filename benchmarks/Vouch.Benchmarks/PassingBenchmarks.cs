using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class PassingBenchmarks
{
    private readonly int _number = 42;
    private readonly string _text = new string('x', 20) + "-end";
    private readonly string _sameText = new string('x', 20) + "-end";
    private readonly List<int> _items = Enumerable.Range(0, 100).ToList();
    private readonly Func<Task> _throws = () => throw new InvalidOperationException("order 17 is missing");

    [Benchmark] public void Vouch_IntBe() => VouchPassing.IntBe(_number, 42);
    [Benchmark] public void Fluent_IntBe() => FluentPassing.IntBe(_number, 42);

    [Benchmark] public void Vouch_StringBe() => VouchPassing.StringBe(_text, _sameText);
    [Benchmark] public void Fluent_StringBe() => FluentPassing.StringBe(_text, _sameText);

    [Benchmark] public void Vouch_HaveCount() => VouchPassing.HaveCount(_items, 100);
    [Benchmark] public void Fluent_HaveCount() => FluentPassing.HaveCount(_items, 100);

    [Benchmark] public void Vouch_Contain() => VouchPassing.Contain(_items, 99);
    [Benchmark] public void Fluent_Contain() => FluentPassing.Contain(_items, 99);

    [Benchmark] public Task Vouch_ThrowAsyncWithMessage() => VouchPassing.ThrowWithMessage(_throws, "order * is missing");
    [Benchmark] public Task Fluent_ThrowAsyncWithMessage() => FluentPassing.ThrowWithMessage(_throws, "order * is missing");
}
