# Contributing

Contributions are welcome, including small ones. This adds to the
[BaryoDev-wide guide](https://github.com/BaryoDev/.github/blob/main/CONTRIBUTING.md); where the two
disagree, this file wins.

## Getting it running

```sh
git clone https://github.com/BaryoDev/Vouch.git
cd Vouch
dotnet test
```

That is the whole setup. No database, no fixture download, no local paths. You need the .NET 8 and
.NET 10 SDKs, because the library and its tests target both.

## What this library is for

One thing: **the `.Should()` surface of FluentAssertions, with no dependencies and no cost on a
passing assertion.** Method names and meanings are shared with FluentAssertions 8. Code is not.

Those constraints decide most design questions:

- **No runtime dependencies.** A pull request that adds a `PackageReference` to
  `src/Vouch/Vouch.csproj` removes the reason this library exists. If something seems to need a
  package, open an issue first.
- **No FluentAssertions code.** Version 8 is under a licence Vouch does not use. Write from the
  method's meaning, not from its source. AwesomeAssertions is referenced only by
  `tests/Vouch.Differential.Tests`, and FluentAssertions only by `benchmarks/Vouch.Benchmarks`.
- **A passing assertion on a value type, string or bool allocates 0 bytes.** Message text,
  formatting and diffs are built only on failure.
- **No stack trace or source file reading at run time.** The expression text comes from
  `[CallerArgumentExpression]`.
- **No test framework reference.** A failure throws `VouchException`.

## Tests

**Every change needs a test that fails without it.** For a bug fix, write the test that reproduces
the bug, watch it fail, then fix it.

**Every public method has three tests:** one that passes, one that fails, and one that pins the
failure message. Vouch's own tests use plain xUnit `Assert`, never Vouch.

Rules that are easy to guess wrong go in `tests/Vouch.Differential.Tests`, which runs the same input
through Vouch and AwesomeAssertions and compares pass and fail. If you change a rule on purpose,
say in the pull request which way it now differs and why.

The worked version of the testing rule is at
[BaryoDev/.github/TESTING.md](https://github.com/BaryoDev/.github/blob/main/TESTING.md). The
question it turns on:

> Name the production change that would make this test fail.

A change to the public surface fails the API approval test in `tests/Vouch.ApiApproval.Tests`. If
the change is intended, update the approved file in the same pull request.

CI builds with warnings as errors, checks `dotnet format --verify-no-changes`, runs `dotnet test`
and runs `samples/check.sh`, which fails one test on purpose in each of xUnit v2, xUnit v3, NUnit
and MSTest to check the failure shows as a plain test failure. All of it has to pass before a merge.

## Writing

Plain and short. No em dashes, no arrow glyphs, no emoji. Commit subjects are short imperative
phrases with no prefixes.

## Good first issues

Look for [`good first issue`](https://github.com/BaryoDev/Vouch/labels/good%20first%20issue). If
nothing fits, an issue with an assertion that passed when it should have failed, or a failure
message that was hard to read, is genuinely useful. Include the smallest code that shows it.

## One entirely optional favour

If this library saved you an afternoon, a star costs you nothing and is the only signal a small
free package has that anyone is using it. It is not a condition of anything: your pull request gets
the same review either way.

## Reporting a security issue

Do not open a public issue. See [SECURITY.md](SECURITY.md).
