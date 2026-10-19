# Benchmark results

Aver against FluentAssertions 8.3.0, BenchmarkDotNet 0.15.8, default job, memory diagnoser on.

Full run pending.

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

Not yet measured. A missed target will be listed here as a gap.
