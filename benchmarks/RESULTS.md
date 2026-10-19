# Benchmark results

Aver against FluentAssertions 8.3.0, BenchmarkDotNet 0.15.8, default job, memory diagnoser on.

Full run on 2026-10-19. Both targets met.

## Machine

- Apple M1, 8 cores
- macOS 27.0.1
- .NET SDK 10.0.103, runtime 10.0.3, Arm64 RyuJIT

## Benchmarks

Passing assertions: `int Be`, `string Be`, `HaveCount` and `Contain` on a 100 item list, `ThrowAsync<T>().WithMessage` with a wildcard.

`BeEquivalentTo`: one record with 6 members and a nested list, a list of 100 such records in reverse order, and a list of 1,000 `Row(long Id, string Name)` records in reverse order (Id starts at 1_700_000_000_000, Name is the same on every row). The last one measures a known weak spot, numbers inside objects hashing coarsely.

## Targets

- 0 bytes on passing value type, string and bool asserts.
- `BeEquivalentTo` at least 5 times faster and 5 times less memory than FluentAssertions 8.3.0.

Both met. Passing `int Be` and `string Be` allocate 0 bytes. `bool` has no benchmark of its own; it goes through the same value type path as `int`. `HaveCount` and `Contain` allocate 40 bytes, the collection wrapper. `BeEquivalentTo` is 37 to 7,617 times faster and uses 111 to 19,719 times less memory.

## Passing assertions

| Assertion | Aver | FluentAssertions | Aver allocated | FluentAssertions allocated |
|---|---:|---:|---:|---:|
| `int Be` | 0.58 ns | 131.6 ns | 0 B | 1,072 B |
| `string Be` | 2.83 ns | 503.4 ns | 0 B | 3,120 B |
| `HaveCount` | 10.5 ns | 172.7 ns | 40 B | 1,312 B |
| `Contain` | 110.9 ns | 355.4 ns | 40 B | 1,600 B |
| `ThrowAsync<T>().WithMessage` | 10.2 us | 27.0 us | 1,064 B | 18,154 B |

## BeEquivalentTo

| Case | Aver | FluentAssertions | Faster | Aver allocated | FluentAssertions allocated | Less memory |
|---|---:|---:|---:|---:|---:|---:|
| One record | 796 ns | 29.2 us | 37x | 960 B | 106,426 B | 111x |
| 100 records, reverse order | 142.5 us | 1,085 ms | 7,617x | 89,200 B | 1,758,949,096 B | 19,719x |
| 1,000 rows, reverse order | 49.0 ms | 5,667 ms | 116x | 24,122,008 B | 10,457,191,256 B | 434x |

Means from BenchmarkDotNet. The raw reports are written to `BenchmarkDotNet.Artifacts/results/`, which is not committed.
