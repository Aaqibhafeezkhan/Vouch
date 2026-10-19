# Aver

Fluent assertions for .NET with no dependencies. The `.Should()` surface of FluentAssertions, under MPL-2.0.

## Install

```sh
dotnet add package Aver
```

Targets `net8.0` and `net10.0`. No runtime dependencies. Aver references no test framework, so it works under xUnit v2, xUnit v3, NUnit and MSTest. A failed assertion throws `AverException`, and each framework shows it as a plain test failure.

## Example

```csharp
using Aver;

order.Total.Should().Be(120m);
order.Lines.Should().HaveCount(3).And.ContainSingle(l => l.Sku == "A-1");
name.Should().StartWith("Juan").And.EndWith("Cruz");

Func<Task> act = () => service.SaveAsync(null!);
(await act.Should().ThrowAsync<ArgumentNullException>()).WithParameterName("order");
```

The expression text in a failure message comes from `[CallerArgumentExpression]` at compile time. Aver never reads a stack trace or a source file.

## Supported assertions

**Any value:** `Be`, `NotBe`, `BeNull`, `NotBeNull`, `BeOfType<T>`, `BeOfType(Type)`, `NotBeOfType<T>`, `BeAssignableTo<T>`, `NotBeAssignableTo<T>`, `BeSameAs`, `NotBeSameAs`, `BeOneOf`, `BeEquivalentTo`, `Match<T>(predicate)`.

**Bool:** `BeTrue`, `BeFalse`, `Be`, `NotBe`.

**Numbers** (`int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, and their nullable forms): `Be`, `NotBe`, `BeGreaterThan`, `BeGreaterThanOrEqualTo`, `BeLessThan`, `BeLessThanOrEqualTo`, `BePositive`, `BeInRange`, `BeOneOf`, `BeApproximately` (decimal, double, float). Other comparable structs such as `TimeSpan` get the four comparisons, and `Guid` gets `NotBeEmpty`.

**Dates** (`DateTime`, `DateTimeOffset`, `DateOnly`, and nullable forms): `Be`, `NotBe`, `BeAfter`, `BeBefore`, `BeOnOrAfter`, `BeOnOrBefore`, `BeCloseTo` (not on `DateOnly`).

**Strings:** `Be` (ordinal, case-sensitive), `NotBe`, `BeEmpty`, `NotBeEmpty`, `BeNull`, `NotBeNull`, `BeNullOrEmpty`, `NotBeNullOrEmpty`, `NotBeNullOrWhiteSpace`, `StartWith`, `NotStartWith`, `EndWith`, `Contain`, `NotContain`, `Match` (wildcards `*` and `?`), `MatchRegex`, `HaveLength`, `BeOneOf`, `BeEquivalentTo`, `NotContainEquivalentOf`.

**Collections:** `BeEmpty`, `NotBeEmpty`, `BeNull`, `NotBeNull`, `BeNullOrEmpty`, `HaveCount`, `HaveCountGreaterThan`, `HaveCountGreaterThanOrEqualTo`, `Contain` (an item, several items, a collection or a predicate), `NotContain`, `ContainSingle`, `OnlyContain`, `Equal`, `NotEqual`, `OnlyHaveUniqueItems`, `BeInAscendingOrder`, `BeInDescendingOrder` (with a key selector or a comparer), `AllSatisfy`, `NotIntersectWith`, `NotContainEquivalentOf`, `BeSubsetOf`, `BeSameAs`, `EndWith`, `BeEquivalentTo` (any order unless `WithStrictOrdering`).

**Dictionaries:** `ContainKey`, `NotContainKey`, `ContainKeys`, plus the collection assertions.

**Delegates:** `Throw<T>`, `ThrowAsync<T>`, `NotThrow`, `NotThrowAsync`. `T` matches a type derived from it. Then `WithMessage` (wildcard pattern), `WithParameterName` and `Which`. `Invoking` works as an extension on any subject and as `FluentActions.Invoking` and `FluentActions.Awaiting`.

**Chains:** `.And` returns to the same assertions. `.Which` gives the matched item from `ContainSingle`, the typed value from `BeOfType<T>` and the exception from `Throw`. `.Subject` gives the value from `NotThrowAsync` on a function with a result.

## What a failure looks like

```
Expected name to be "Juan Dela Cruz", but found "Juan Dela Kruz".
  They differ at index 10: "Cruz" against "Kruz".
```

```
Expected xs to equal the expected items, but it has 4 items, expected 3.
  They differ at index 2: expected 3, found 9.
```

```
Expected act to throw InvalidOperationException, but it threw ArgumentException:
  "Name is required."
```

`BeEquivalentTo` lists every difference with its member path:

```
Expected package to be equivalent to the expected Bundle, but:
  Name was "Alpha", expected "Alpha basic".
  Tests[1] expected "AAA" was not found.
```

Messages are built only when an assertion fails. A passing assertion on a value type, string or bool allocates 0 bytes.

## Moving from FluentAssertions

1. Change the package reference from `FluentAssertions` to `Aver`.
2. Change `using FluentAssertions;` to `using Aver;`.

Aver shares method names and meanings with FluentAssertions 8. It does not copy any of its code, and its message text is its own.

Not supported:

- `AssertionScope`.
- `becauseArgs` format arguments. The reason is one plain string.
- `BeEquivalentTo` options other than `WithStrictOrdering`.
- Global `AssertionOptions`, custom assertion extension points, and the time helpers (`5.Minutes()`, `.March(2026)`).

### Reason strings

Most assertions take an optional `string because`. Aver appends ` Because {reason}.` to the failure message (a period is added if missing). A null or blank reason changes nothing, and a passing assertion pays nothing.

The reason overload exists on:

- Any value: `Be`, `NotBe`, `BeNull`, `NotBeNull`, `BeOfType`, `BeAssignableTo`, `Match`.
- Bool: `BeTrue`, `BeFalse`, `Be`, `NotBe`.
- Numbers: `Be`, `NotBe`, the four comparisons, `BePositive`, `BeInRange`.
- Strings: `Be`, `NotBe`, `BeEmpty`, `NotBeEmpty`, `BeNull`, `NotBeNull`, `BeNullOrEmpty`, `NotBeNullOrEmpty`, `NotBeNullOrWhiteSpace`, `StartWith`, `NotStartWith`, `EndWith`, `Contain`, `NotContain`, `Match`, `MatchRegex`, `HaveLength`, `BeOneOf`, `BeEquivalentTo`, `NotContainEquivalentOf`.
- Collections: `BeEmpty`, `NotBeEmpty`, `BeNull`, `NotBeNull`, `BeNullOrEmpty`, `HaveCount`, `Contain`, `NotContain`, `ContainSingle`, `OnlyContain`, `Equal`, `BeEquivalentTo`.
- Dictionaries: `ContainKey`, `NotContainKey`.
- Delegates: `Throw`, `NotThrow`, `ThrowAsync`, `NotThrowAsync`.

It does not exist on the date assertions, `BeApproximately`, `BeSameAs`, `NotBeSameAs`, `NotBeOfType`, `NotBeAssignableTo`, `ContainKeys`, or the other collection assertions (`HaveCountGreaterThan`, `NotEqual`, `OnlyHaveUniqueItems`, the ordering checks, `AllSatisfy`, `NotIntersectWith`, `BeSubsetOf`, `BeSameAs`, `EndWith`).

`Contain(T, because)` and `NotContain(T, because)` exist and win over the `params` form, as in FluentAssertions. On a collection of strings, `Contain("a", "b")` and `NotContain("a", "b")` read the second string as the reason, not as a second item. To check several items, pass a collection: `Contain(["a", "b"])`.

### Where Aver differs on purpose

Aver was checked against AwesomeAssertions (the Apache-2.0 fork of FluentAssertions 8) with a differential test project that runs edge inputs through both libraries and compares pass and fail. It was also tried on a large existing FluentAssertions test suite, switched over by changing the package and the `using`. Where Aver differs from FluentAssertions:

- The reason sits at the end of the message. FluentAssertions places it mid-sentence.
- `Match<T>` on a null subject or a subject of another type fails with a message. FluentAssertions throws `NullReferenceException` or `InvalidCastException`.
- `Which` after a `Throw<T>` that matched several inner exceptions fails with a plain message.
- `WithMessage` passes when any matching inner exception matches.
- `Throw<T>` and `ThrowAsync<T>` look through the inner exceptions of an `AggregateException` (flattened) before checking the exception itself. An empty `AggregateException` fails `Throw<Exception>`.
- `WithMessage` ignores case and newline differences. An empty pattern throws `ArgumentException`.
- String `Match` with an empty pattern throws `ArgumentException`, and one trailing newline on the subject is ignored.
- `Be`, `NotBe` and `BeEquivalentTo` on objects treat numbers of different types with the same value as equal (`1` and `1L`, `1` and `1.0`). `BeOneOf` uses plain `Equals`, so mixed types do not match.
- A `decimal` equals a `double` or `float` only when both round trips match. `NaN` and overflow never equal, except that a `float` `NaN` equals a `double` `NaN`.
- String `BeEquivalentTo(null)` does not throw. Null equals null only.
- `BeEquivalentTo` detects cycles and has a depth limit, so a graph that refers to itself fails with a message.

## Benchmarks

`benchmarks/Aver.Benchmarks` compares Aver with FluentAssertions 8.3.0 using BenchmarkDotNet. It covers passing `int Be`, `string Be`, `HaveCount` and `Contain` on a 100 item list, `ThrowAsync<T>().WithMessage`, and `BeEquivalentTo` on one record with 6 members, a list of 100 records in reverse order, and a list of 1,000 `Row(long Id, string Name)` records in reverse order.

Targets: 0 bytes on passing value type, string and bool assertions, and `BeEquivalentTo` at least 5 times faster and 5 times lighter on memory than FluentAssertions. The full run is pending, so no numbers are published yet. See [benchmarks/RESULTS.md](benchmarks/RESULTS.md). A missed target will be listed there as a gap.

## Licence

[MPL-2.0](LICENSE).
