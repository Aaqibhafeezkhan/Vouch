# Consumer gaps

A real FluentAssertions 8 test suite (8 test projects, 381 files) was copied, switched to Aver by replacing the `using` and the package reference, and compiled. The first build gave 201 distinct errors. The last gives 0. This file lists each gap by API shape. No consumer code is quoted.

First build, by code: CS1501 123, CS1061 53, CS1503 9, CS0411 5, CS0103 5, CS0029 3, CS1660 2, CS1729 1.

## Reason strings (about 150 call sites)

The spec listed "because" arguments as a non-goal on the belief that the consumer used none. It passes a positional reason string in about 150 calls. Aver now has a `string because` overload next to each assertion the suite uses, and next to its siblings: Be, NotBe, BeTrue, BeFalse, BeNull, NotBeNull, BeEmpty, NotBeEmpty, BeNullOrEmpty, the numeric comparisons, HaveCount, Contain, NotContain, ContainSingle, OnlyContain, Equal, BeEquivalentTo, BeOfType, BeAssignableTo, StartWith, BeOneOf, ContainKey, NotContainKey, Throw, NotThrow, ThrowAsync, NotThrowAsync.

A failure message gets " Because {reason}." appended (a period is added if missing). A null or blank reason changes nothing. A passing assertion pays nothing. `becauseArgs` format arguments were not added, no call site uses them.

`Contain(T, string because)` is the normal form and wins over `Contain(params T[])`, as in FluentAssertions. On a collection of strings, `Contain("a", "b")` and `NotContain("a", "b")` read the second string as the reason, not as a second item. Three call sites in the suite rely on this. The same holds in FluentAssertions. To check several items, pass a collection: `Contain(["a", "b"])`.

Other methods that take `params` (Dates, BeApproximately, BeOneOf with params) have no reason overload.

## Other gaps

| Shape | Sites | Fix |
| --- | --- | --- |
| `Invoking` as an extension on any subject, with Action and Func forms | 33 | `Invoking<T>(this T, Action<T>)` and `Invoking<T, TResult>(this T, Func<T, TResult>)` |
| Static `Invoking` and `Awaiting` helpers | 5 | `FluentActions`, same name as the FluentActions type |
| `Match<T>(Expression<Func<T, bool>>)` on an object | 13 | `ObjectAssertions.Match<T>`. A null subject fails. A predicate exception propagates |
| `BeInAscendingOrder(IComparer<T>)` | 5 | overloads for ascending and descending |
| `NotContainEquivalentOf` on a string | 3 | ordinal, case-insensitive |
| `WhoseValue` on the which-constraint | 3 | alias of `Which` |
| `WithParameterName` on a thrown ArgumentException, sync and on the task | 2 | `ArgumentExceptionExtensions` |
| `BeOfType(Type)` non-generic | few | `ObjectAssertions.BeOfType(Type)` |
| `BeSameAs` on a collection | few | `CollectionAssertionsBase.BeSameAs` |
| `EndWith` on a collection | few | `EndWith(IEnumerable<T>)` |
| `BeNullOrEmpty` on a collection or dictionary | few | added with a reason overload |
| Ordering comparisons on `TimeSpan` and other comparable structs | few | `ComparableValueAssertions` |
| `NotBeEmpty` on `Guid` | few | `ValueAssertions<Guid>.NotBeEmpty` |
| `Be` taking a nullable expected value on a nullable subject | 2 | `NullableValueAssertions<T>.Be(T?)`. Null equals null |
| `NotThrowAsync` on an async function with a result, then `.Subject` | 1 | `AsyncFunctionAssertions<T>`. `Should()` on `Func<Task<T>>` now returns it |

## Not closed

None.

## Checked against AwesomeAssertions

`tests/Aver.Differential.Tests` runs edge inputs through both libraries and compares pass and fail. Where they differ on purpose:

- Where the reason sits in the message. Aver appends it at the end. AwesomeAssertions places it mid-sentence.
- `Match<T>` on a null subject or a subject of another type. AwesomeAssertions throws `NullReferenceException` or `InvalidCastException`. Aver fails with a message.
- `Which` after a `Throw<T>` that matched several inner exceptions fails with a plain message. AwesomeAssertions throws from `Which` too. `WithMessage` passes when any matching inner exception matches.
