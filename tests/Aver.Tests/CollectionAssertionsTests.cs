using Xunit;

namespace Aver.Tests;

public class CollectionAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<AverException>(act).Message;

    private static readonly Row[] Rows = [new("FBS", 1), new("CREA", 2)];

    // resolution and single pass

    [Fact] public void String_still_resolves_to_StringAssertions() { StringAssertions s = "abc".Should(); Assert.IsType<StringAssertions>("abc".Should()); s.Be("abc"); }
    [Fact] public void List_resolves_to_collection_assertions() => Assert.IsType<GenericCollectionAssertions<int>>(new List<int>().Should());
    [Fact] public void Array_resolves_to_collection_assertions() => Assert.IsType<GenericCollectionAssertions<int>>(new[] { 1 }.Should());

    [Fact]
    public void Subject_is_enumerated_once()
    {
        var calls = 0;
        IEnumerable<int> Gen() { calls++; yield return 1; }
        Gen().Should().HaveCount(1).And.Contain(1);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Single_pass_generator_failure_shows_the_items()
    {
        var calls = 0;
        IEnumerable<int> Gen() { if (++calls > 1) { throw new InvalidOperationException("twice"); } yield return 4; yield return 5; }
        var gen = Gen();
        Assert.Equal("Expected gen to contain 3 items, but found 2:\n  [0] 4\n  [1] 5", Message(() => gen.Should().HaveCount(3)));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Single_pass_generator_works_with_Equal_and_Contain_and_ordering()
    {
        IEnumerable<int> Gen() { yield return 1; yield return 2; }
        var expectedCalls = 0;
        IEnumerable<int> Expected() { if (++expectedCalls > 1) { throw new InvalidOperationException("twice"); } yield return 1; yield return 3; }
        var gen = Gen();
        var e = Expected();
        Assert.Throws<AverException>(() => gen.Should().Equal(e));
        Assert.Equal(1, expectedCalls);
    }

    // null arguments throw first, null subject fails

    [Fact] public void Null_expected_collection_throws_before_subject_check() { List<int>? xs = null; Assert.Throws<ArgumentNullException>(() => xs.Should().Equal((IEnumerable<int>)null!)); }
    [Fact] public void Null_params_array_throws() => Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().Equal((int[])null!));
    [Fact] public void Null_predicate_throws() { List<int>? xs = null; Assert.Throws<ArgumentNullException>(() => xs.Should().Contain((System.Linq.Expressions.Expression<Func<int, bool>>)null!)); }
    [Fact] public void Null_action_throws() => Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().AllSatisfy(null!));
    [Fact] public void Null_key_selector_throws() => Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeInAscendingOrder<int>(null!));
    [Fact] public void Null_other_collection_throws() { Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeSubsetOf(null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().NotIntersectWith(null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().NotContain((IEnumerable<int>)null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().NotEqual(null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().Contain((IEnumerable<int>)null!)); }

    [Fact] public void Null_list_HaveCount_fails() { List<int>? rows = null; Assert.Equal("Expected rows to contain 3 items, but found null.", Message(() => rows.Should().HaveCount(3))); }
    [Fact] public void Null_list_BeEmpty_fails() { List<int>? rows = null; Assert.Equal("Expected rows to be empty, but found null.", Message(() => rows.Should().BeEmpty())); }
    [Fact] public void Null_list_NotBeEmpty_fails() { List<int>? rows = null; Assert.Equal("Expected rows not to be empty, but found null.", Message(() => rows.Should().NotBeEmpty())); }
    [Fact] public void Null_list_BeNull_passes() { List<int>? rows = null; rows.Should().BeNull(); }
    [Fact] public void Null_list_NotBeNull_fails() { List<int>? rows = null; Assert.Equal("Expected rows not to be null, but it was.", Message(() => rows.Should().NotBeNull())); }
    [Fact]
    public void Null_list_never_throws_NullReference()
    {
        List<int>? rows = null;
        Assert.Throws<AverException>(() => rows.Should().Contain(1));
        Assert.Throws<AverException>(() => rows.Should().Contain(1, 2));
        Assert.Throws<AverException>(() => rows.Should().Contain(x => x > 0));
        Assert.Throws<AverException>(() => rows.Should().NotContain(1));
        Assert.Throws<AverException>(() => rows.Should().ContainSingle());
        Assert.Throws<AverException>(() => rows.Should().OnlyContain(x => x > 0));
        Assert.Throws<AverException>(() => rows.Should().Equal(1));
        Assert.Throws<AverException>(() => rows.Should().NotEqual(new[] { 1 }));
        Assert.Throws<AverException>(() => rows.Should().OnlyHaveUniqueItems());
        Assert.Throws<AverException>(() => rows.Should().BeInAscendingOrder());
        Assert.Throws<AverException>(() => rows.Should().BeInDescendingOrder());
        Assert.Throws<AverException>(() => rows.Should().AllSatisfy(x => { }));
        Assert.Throws<AverException>(() => rows.Should().BeSubsetOf(new[] { 1 }));
        Assert.Throws<AverException>(() => rows.Should().NotIntersectWith(new[] { 1 }));
        Assert.Throws<AverException>(() => rows.Should().HaveCountGreaterThan(1));
        Assert.Throws<AverException>(() => rows.Should().HaveCountGreaterThanOrEqualTo(1));
    }

    // BeEmpty / NotBeEmpty / BeNull / NotBeNull

    [Fact] public void BeEmpty_passes() => new List<int>().Should().BeEmpty();
    [Fact] public void BeEmpty_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().BeEmpty());
    [Fact] public void BeEmpty_message() { var xs = new[] { "FBS" }; Assert.Equal("Expected xs to be empty, but found 1 item:\n  [0] \"FBS\"", Message(() => xs.Should().BeEmpty())); }
    [Fact] public void BeEmpty_message_plural() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to be empty, but found 2 items:\n  [0] 1\n  [1] 2", Message(() => xs.Should().BeEmpty())); }
    [Fact] public void NotBeEmpty_passes() => new[] { 1 }.Should().NotBeEmpty();
    [Fact] public void NotBeEmpty_fails() => Assert.Throws<AverException>(() => new int[0].Should().NotBeEmpty());
    [Fact] public void NotBeEmpty_message() { var xs = new int[0]; Assert.Equal("Expected xs not to be empty, but it was.", Message(() => xs.Should().NotBeEmpty())); }
    [Fact] public void BeNull_passes() { List<int>? xs = null; xs.Should().BeNull(); }
    [Fact] public void BeNull_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().BeNull());
    [Fact] public void BeNull_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to be null, but found [1].", Message(() => xs.Should().BeNull())); }
    [Fact] public void NotBeNull_passes() => new[] { 1 }.Should().NotBeNull();
    [Fact] public void NotBeNull_fails() { int[]? xs = null; Assert.Throws<AverException>(() => xs.Should().NotBeNull()); }

    // counts

    [Fact] public void HaveCount_passes() => new[] { 1, 2 }.Should().HaveCount(2);
    [Fact] public void HaveCount_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().HaveCount(2));
    [Fact] public void HaveCount_message() { var xs = new[] { "a", "b" }; Assert.Equal("Expected xs to contain 3 items, but found 2:\n  [0] \"a\"\n  [1] \"b\"", Message(() => xs.Should().HaveCount(3))); }
    [Fact] public void HaveCount_singular_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain 1 item, but found 0.", Message(() => xs.Should().HaveCount(1))); }
    [Fact]
    public void Message_lists_items_and_cuts_at_ten()
    {
        var rows = Enumerable.Range(0, 50).ToList();
        var msg = Message(() => rows.Should().HaveCount(3));
        Assert.Contains("  [9] 9", msg);
        Assert.DoesNotContain("  [10]", msg);
        Assert.EndsWith("\n  ... (40 more)", msg);
    }
    [Fact] public void HaveCountGreaterThan_passes() => new[] { 1, 2 }.Should().HaveCountGreaterThan(1);
    [Fact] public void HaveCountGreaterThan_fails_on_equal() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().HaveCountGreaterThan(2));
    [Fact] public void HaveCountGreaterThan_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain more than 1 item, but found 1:\n  [0] 1", Message(() => xs.Should().HaveCountGreaterThan(1))); }
    [Fact] public void HaveCountGreaterThanOrEqualTo_passes() => new[] { 1, 2 }.Should().HaveCountGreaterThanOrEqualTo(2);
    [Fact] public void HaveCountGreaterThanOrEqualTo_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().HaveCountGreaterThanOrEqualTo(2));
    [Fact] public void HaveCountGreaterThanOrEqualTo_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain at least 2 items, but found 1:\n  [0] 1", Message(() => xs.Should().HaveCountGreaterThanOrEqualTo(2))); }

    // Contain

    [Fact] public void Contain_item_passes() => new[] { 1, 2 }.Should().Contain(2);
    [Fact] public void Contain_item_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().Contain(2));
    [Fact] public void Contain_item_message() { var xs = new[] { "A", "B" }; Assert.Equal("Expected xs to contain \"FBS\", but it did not. It has:\n  [0] \"A\"\n  [1] \"B\"", Message(() => xs.Should().Contain("FBS"))); }
    [Fact] public void Contain_item_Which_is_the_item() => Assert.Equal(2, new[] { 1, 2 }.Should().Contain(2).Which);
    [Fact] public void Contain_item_allows_null_item() => new string?[] { "a", null }.Should().Contain((string?)null);
    [Fact] public void Contain_items_passes() => new[] { 1, 2, 3 }.Should().Contain(new List<int> { 1, 3 });
    [Fact] public void Contain_items_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().Contain(new List<int> { 1, 3 }));
    [Fact] public void Contain_items_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain all of [1, 3, 4], but it is missing [3, 4]. It has:\n  [0] 1", Message(() => xs.Should().Contain(new List<int> { 1, 3, 4 }))); }
    [Fact] public void Contain_params_passes() => new[] { 1, 2, 3 }.Should().Contain(1, 3);
    [Fact] public void Contain_params_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().Contain(1, 3));
    [Fact] public void Contain_params_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain all of [1, 3], but it is missing [3]. It has:\n  [0] 1", Message(() => xs.Should().Contain(1, 3))); }
    [Fact] public void Contain_predicate_passes() => new[] { 1, 2 }.Should().Contain(x => x > 1);
    [Fact] public void Contain_predicate_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().Contain(x => x > 1));
    [Fact] public void Contain_predicate_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain an item matching (x > 1), but it did not. It has:\n  [0] 1", Message(() => xs.Should().Contain(x => x > 1))); }
    [Fact] public void Contain_predicate_Which_is_first_match() => Assert.Equal(new Row("CREA", 2), Rows.Should().Contain(r => r.Qty > 1).Which);

    // NotContain

    [Fact] public void NotContain_item_passes() => new[] { 1 }.Should().NotContain(2);
    [Fact] public void NotContain_item_fails() => Assert.Throws<AverException>(() => new[] { 1 }.Should().NotContain(1));
    [Fact] public void NotContain_item_message() { var xs = new[] { 5, 6 }; Assert.Equal("Expected xs not to contain 6, but found it at index 1.", Message(() => xs.Should().NotContain(6))); }
    [Fact] public void NotContain_items_passes() => new[] { 1 }.Should().NotContain(new List<int> { 2, 3 });
    [Fact] public void NotContain_items_fails() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().NotContain(new List<int> { 2, 3 }));
    [Fact] public void NotContain_items_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs not to contain any of [2, 3], but found [2].", Message(() => xs.Should().NotContain(new List<int> { 2, 3 }))); }
    [Fact] public void NotContain_predicate_passes() => new[] { 1 }.Should().NotContain(x => x > 1);
    [Fact] public void NotContain_predicate_fails() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().NotContain(x => x > 1));
    [Fact] public void NotContain_predicate_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs not to contain any item matching (x > 1), but found:\n  [1] 2\n  [2] 3", Message(() => xs.Should().NotContain(x => x > 1))); }

    // ContainSingle

    [Fact] public void ContainSingle_passes() => new[] { 1 }.Should().ContainSingle();
    [Fact] public void ContainSingle_fails() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().ContainSingle());
    [Fact] public void ContainSingle_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain a single item, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().ContainSingle())); }
    [Fact] public void ContainSingle_message_on_empty() { var xs = new int[0]; Assert.Equal("Expected xs to contain a single item, but found 0.", Message(() => xs.Should().ContainSingle())); }
    [Fact] public void ContainSingle_Which_is_the_item() => Assert.Equal("FBS", new[] { new Row("FBS", 1) }.Should().ContainSingle().Which.Code);
    [Fact] public void ContainSingle_Subject_is_the_item() => Assert.Equal(1, new[] { 1 }.Should().ContainSingle().Subject);
    [Fact] public void ContainSingle_predicate_passes() => Assert.Equal("CREA", Rows.Should().ContainSingle(r => r.Qty == 2).Which.Code);
    [Fact] public void ContainSingle_predicate_fails_on_two() => Assert.Throws<AverException>(() => new[] { 1, 1 }.Should().ContainSingle(x => x == 1));
    [Fact] public void ContainSingle_predicate_fails_on_none() => Assert.Throws<AverException>(() => new[] { 1 }.Should().ContainSingle(x => x == 2));
    [Fact] public void ContainSingle_predicate_message() { var xs = new[] { new Row("FBS", 1), new Row("CREA", 2), new Row("FBS", 3) }; Assert.Equal("Expected xs to contain a single item matching (x.Code == \"FBS\"), but found 2:\n  [0] Row { Code = \"FBS\", Qty = 1 }\n  [2] Row { Code = \"FBS\", Qty = 3 }", Message(() => xs.Should().ContainSingle(x => x.Code == "FBS"))); }
    [Fact] public void ContainSingle_predicate_message_on_none() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain a single item matching (x == 2), but found 0. It has:\n  [0] 1", Message(() => xs.Should().ContainSingle(x => x == 2))); }

    // OnlyContain

    [Fact] public void OnlyContain_passes() => new[] { 1, 2 }.Should().OnlyContain(x => x > 0);
    [Fact] public void OnlyContain_fails() => Assert.Throws<AverException>(() => new[] { 1, -2 }.Should().OnlyContain(x => x > 0));
    [Fact] public void OnlyContain_message() { var xs = new[] { 1, -2, 0 }; Assert.Equal("Expected xs to only contain items matching (x > 0), but these did not:\n  [1] -2\n  [2] 0", Message(() => xs.Should().OnlyContain(x => x > 0))); }

    // Equal / NotEqual

    [Fact] public void Equal_passes() => new[] { 1, 2 }.Should().Equal(new List<int> { 1, 2 });
    [Fact] public void Equal_params_form() => new List<string> { "a", "b" }.Should().Equal("a", "b");
    [Fact] public void Equal_is_ordered() => Assert.Throws<AverException>(() => new[] { 2, 1 }.Should().Equal(1, 2));
    [Fact] public void Equal_message_differs_at_index() { var xs = new[] { "FBS", "LDL", "HBA1C" }; Assert.Equal("Expected xs to equal the expected items, but they differ at index 2: expected \"CREA\", found \"HBA1C\".", Message(() => xs.Should().Equal("FBS", "LDL", "CREA"))); }
    [Fact] public void Equal_message_subject_longer() { var xs = new[] { 1, 2, 9, 4 }; Assert.Equal("Expected xs to equal the expected items, but it has 4 items, expected 3.\n  They differ at index 2: expected 3, found 9.", Message(() => xs.Should().Equal(1, 2, 3))); }
    [Fact] public void Equal_message_subject_shorter_prefix_matches() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to equal the expected items, but it has 2 items, expected 3.\n  They differ at index 2: expected 3, found nothing.", Message(() => xs.Should().Equal(1, 2, 3))); }
    [Fact] public void Equal_message_subject_longer_prefix_matches() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to equal the expected items, but it has 3 items, expected 2.\n  They differ at index 2: expected nothing, found 3.", Message(() => xs.Should().Equal(1, 2))); }
    [Fact] public void Equal_handles_null_items() => new string?[] { "a", null }.Should().Equal("a", null);
    [Fact] public void NotEqual_passes() => new[] { 1, 2 }.Should().NotEqual(new[] { 2, 1 });
    [Fact] public void NotEqual_fails() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().NotEqual(new[] { 1, 2 }));
    [Fact] public void NotEqual_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs not to equal [1, 2], but it did.", Message(() => xs.Should().NotEqual(new[] { 1, 2 }))); }

    // uniqueness

    [Fact] public void OnlyHaveUniqueItems_passes() => new[] { 1, 2 }.Should().OnlyHaveUniqueItems();
    [Fact] public void OnlyHaveUniqueItems_fails() => Assert.Throws<AverException>(() => new[] { 1, 1 }.Should().OnlyHaveUniqueItems());
    [Fact] public void OnlyHaveUniqueItems_message() { var xs = new[] { "FBS", "LDL", "FBS" }; Assert.Equal("Expected xs to have unique items, but \"FBS\" appears 2 times.", Message(() => xs.Should().OnlyHaveUniqueItems())); }
    [Fact] public void OnlyHaveUniqueItems_handles_null_items() { Assert.Throws<AverException>(() => new string?[] { null, null }.Should().OnlyHaveUniqueItems()); new string?[] { null, "a" }.Should().OnlyHaveUniqueItems(); }

    // ordering

    [Fact] public void BeInAscendingOrder_passes() => new[] { 1, 2, 2, 3 }.Should().BeInAscendingOrder();
    [Fact] public void BeInAscendingOrder_fails() => Assert.Throws<AverException>(() => new[] { 2, 1 }.Should().BeInAscendingOrder());
    [Fact] public void BeInAscendingOrder_message() { var xs = new[] { 1, 3, 7, 5 }; Assert.Equal("Expected xs to be in ascending order, but [3] 5 comes after [2] 7.", Message(() => xs.Should().BeInAscendingOrder())); }
    [Fact] public void BeInAscendingOrder_key_passes() => Rows.Should().BeInAscendingOrder(r => r.Qty);
    [Fact] public void BeInAscendingOrder_key_fails() => Assert.Throws<AverException>(() => Rows.Should().BeInAscendingOrder(r => r.Code));
    [Fact] public void BeInAscendingOrder_key_message() { Assert.Equal("Expected Rows to be in ascending order, but [1] Row { Code = \"CREA\", Qty = 2 } comes after [0] Row { Code = \"FBS\", Qty = 1 }.", Message(() => Rows.Should().BeInAscendingOrder(r => r.Code))); }
    [Fact] public void BeInDescendingOrder_passes() => new[] { 3, 2, 2, 1 }.Should().BeInDescendingOrder();
    [Fact] public void BeInDescendingOrder_fails() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().BeInDescendingOrder());
    [Fact] public void BeInDescendingOrder_message() { var xs = new[] { 9, 7, 8 }; Assert.Equal("Expected xs to be in descending order, but [2] 8 comes after [1] 7.", Message(() => xs.Should().BeInDescendingOrder())); }
    [Fact] public void BeInDescendingOrder_key_passes() => Rows.Should().BeInDescendingOrder(r => r.Code);
    [Fact] public void BeInDescendingOrder_key_fails() => Assert.Throws<AverException>(() => Rows.Should().BeInDescendingOrder(r => r.Qty));
    [Fact] public void BeInDescendingOrder_key_message() { Assert.Equal("Expected Rows to be in descending order, but [1] Row { Code = \"CREA\", Qty = 2 } comes after [0] Row { Code = \"FBS\", Qty = 1 }.", Message(() => Rows.Should().BeInDescendingOrder(r => r.Qty))); }
    [Fact] public void Ordering_passes_on_empty_and_single() { new int[0].Should().BeInAscendingOrder(); new[] { 1 }.Should().BeInDescendingOrder(); }

    // AllSatisfy

    [Fact] public void AllSatisfy_passes() => new[] { 1, 2 }.Should().AllSatisfy(x => x.Should().BePositive());
    [Fact] public void AllSatisfy_fails() => Assert.Throws<AverException>(() => new[] { 1, -1 }.Should().AllSatisfy(x => x.Should().BePositive()));
    [Fact]
    public void AllSatisfy_reports_inner_failure()
    {
        var xs = new[] { 1, -1 };
        var msg = Message(() => xs.Should().AllSatisfy(x => x.Should().BePositive()));
        Assert.Contains("[1]", msg);
    }
    [Fact]
    public void AllSatisfy_message()
    {
        var xs = new[] { 1, -1, 5, 0 };
        Assert.Equal("Expected xs to have all items satisfy the assertion, but 2 did not:\n  [1] Expected x to be positive, but found -1.\n  [3] Expected x to be positive, but found 0.",
            Message(() => xs.Should().AllSatisfy(x => x.Should().BePositive())));
    }
    [Fact]
    public void AllSatisfy_cuts_at_ten()
    {
        var xs = Enumerable.Repeat(-1, 15).ToArray();
        var msg = Message(() => xs.Should().AllSatisfy(x => x.Should().BePositive()));
        Assert.Contains("but 15 did not:", msg);
        Assert.EndsWith("\n  ... (5 more)", msg);
    }
    [Fact] public void AllSatisfy_does_not_swallow_other_exceptions() => Assert.Throws<InvalidOperationException>(() => new[] { 1 }.Should().AllSatisfy(_ => throw new InvalidOperationException()));

    // sets

    [Fact] public void NotIntersectWith_passes() => new[] { 1, 2 }.Should().NotIntersectWith(new[] { 3 });
    [Fact] public void NotIntersectWith_fails() => Assert.Throws<AverException>(() => new[] { 1, 2 }.Should().NotIntersectWith(new[] { 2 }));
    [Fact] public void NotIntersectWith_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs not to intersect with [2, 3, 4], but found common items [2, 3].", Message(() => xs.Should().NotIntersectWith(new[] { 2, 3, 4 }))); }
    [Fact] public void BeSubsetOf_passes() => new[] { 1, 2 }.Should().BeSubsetOf(new[] { 1, 2, 3 });
    [Fact] public void BeSubsetOf_fails() => Assert.Throws<AverException>(() => new[] { 1, 4 }.Should().BeSubsetOf(new[] { 1, 2, 3 }));
    [Fact] public void BeSubsetOf_message() { var xs = new[] { 1, 4, 5 }; Assert.Equal("Expected xs to be a subset of [1, 2, 3], but [4, 5] are not in it.", Message(() => xs.Should().BeSubsetOf(new[] { 1, 2, 3 }))); }
    [Fact] public void BeSubsetOf_single_pass_other() { IEnumerable<int> Other() { yield return 1; yield return 2; } new[] { 1 }.Should().BeSubsetOf(Other()); }

    // chaining

    [Fact] public void And_chains_back_to_the_assertions() => new[] { 1, 2 }.Should().HaveCount(2).And.Contain(1).And.NotContain(3);

    // empty subjects end with a period

    [Fact] public void Contain_item_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain 5, but it was empty.", Message(() => xs.Should().Contain(5))); }
    [Fact] public void Contain_predicate_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain an item matching (x > 1), but it was empty.", Message(() => xs.Should().Contain(x => x > 1))); }
    [Fact] public void Contain_items_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain all of [1, 2], but it is missing [1, 2].", Message(() => xs.Should().Contain(1, 2))); }
    [Fact] public void HaveCountGreaterThan_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain more than 1 item, but found 0.", Message(() => xs.Should().HaveCountGreaterThan(1))); }
    [Fact] public void ContainSingle_predicate_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain a single item matching (x == 2), but found 0.", Message(() => xs.Should().ContainSingle(x => x == 2))); }

    // lazy materialisation

    [Fact]
    public void Failure_on_lazy_set_shows_items_and_enumerates_once()
    {
        var calls = 0;
        IEnumerable<int> Gen() { calls++; yield return 1; }
        var gen = Gen();
        var a = gen.Should();
        Assert.Equal("Expected gen to contain 2 items, but found 1:\n  [0] 1", Message(() => a.HaveCount(2)));
        a.Contain(1);
        Assert.Equal(1, calls);
    }

    // non-comparable items

    private sealed class Opaque { }

    [Fact] public void BeInAscendingOrder_non_comparable_fails_cleanly() { var xs = new[] { new Opaque(), new Opaque() }; Assert.Equal("Expected xs to be in ascending order, but Opaque is not comparable.", Message(() => xs.Should().BeInAscendingOrder())); }
    [Fact] public void BeInDescendingOrder_non_comparable_fails_cleanly() { var xs = new[] { new Opaque(), new Opaque() }; Assert.Equal("Expected xs to be in descending order, but Opaque is not comparable.", Message(() => xs.Should().BeInDescendingOrder())); }
    [Fact] public void BeInAscendingOrder_non_comparable_key_fails_cleanly() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to be in ascending order, but Opaque is not comparable.", Message(() => xs.Should().BeInAscendingOrder(_ => new Opaque()))); }
    [Fact] public void BeInDescendingOrder_non_comparable_key_fails_cleanly() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to be in descending order, but Opaque is not comparable.", Message(() => xs.Should().BeInDescendingOrder(_ => new Opaque()))); }

    [Fact] public void BeSameAs_passes_on_the_same_instance() { var xs = new List<int> { 1 }; xs.Should().BeSameAs(xs); }
    [Fact] public void BeSameAs_fails_on_an_equal_copy() { var xs = new List<int> { 1 }; Assert.Throws<AverException>(() => xs.Should().BeSameAs(new List<int> { 1 })); }
    [Fact] public void BeSameAs_message() { var xs = new List<int> { 1 }; var ys = new List<int> { 2 }; Assert.Equal("Expected xs to refer to [2], but found another collection: [1].", Message(() => xs.Should().BeSameAs(ys))); }
    [Fact] public void BeSameAs_fails_on_a_null_subject() { List<int>? xs = null; Assert.Equal("Expected xs to refer to [2], but found null.", Message(() => xs.Should().BeSameAs(new List<int> { 2 }))); }
    [Fact] public void BeSameAs_null_expected_throws_ArgumentNullException() { var xs = new List<int> { 1 }; Assert.Throws<ArgumentNullException>(() => xs.Should().BeSameAs(null!)); }

    [Fact] public void EndWith_passes_on_the_trailing_items() { new[] { 1, 2, 3 }.Should().EndWith(new[] { 2, 3 }); }
    [Fact] public void EndWith_passes_on_the_whole_sequence_and_on_none() { var xs = new[] { 1, 2 }; xs.Should().EndWith(new[] { 1, 2 }); xs.Should().EndWith(Array.Empty<int>()); }
    [Fact] public void EndWith_fails_on_a_different_tail() { var xs = new[] { 1, 2, 3 }; Assert.Throws<AverException>(() => xs.Should().EndWith(new[] { 1, 3 })); }
    [Fact] public void EndWith_fails_when_the_expected_is_longer() { var xs = new[] { 2, 3 }; Assert.Throws<AverException>(() => xs.Should().EndWith(new[] { 1, 2, 3 })); }
    [Fact] public void EndWith_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to end with [1, 3], but found [1, 2, 3].", Message(() => xs.Should().EndWith(new[] { 1, 3 }))); }
    [Fact] public void EndWith_fails_on_a_null_subject() { int[]? xs = null; Assert.Equal("Expected xs to end with [1], but found null.", Message(() => xs.Should().EndWith(new[] { 1 }))); }
    [Fact] public void EndWith_null_expected_throws_ArgumentNullException() { var xs = new[] { 1 }; Assert.Throws<ArgumentNullException>(() => xs.Should().EndWith(null!)); }
    [Fact] public void EndWith_reads_a_one_shot_sequence_once() { var reads = 0; IEnumerable<int> Tail() { reads++; yield return 3; } new[] { 1, 2, 3 }.Should().EndWith(Tail()); Assert.Equal(1, reads); }

    [Fact] public void BeNullOrEmpty_passes_on_null_and_empty() { List<int>? none = null; none.Should().BeNullOrEmpty(); new List<int>().Should().BeNullOrEmpty(); }
    [Fact] public void BeNullOrEmpty_fails_on_items() { var xs = new List<int> { 1 }; Assert.Throws<AverException>(() => xs.Should().BeNullOrEmpty()); }
    [Fact] public void BeNullOrEmpty_message() { var xs = new List<int> { 1, 2 }; Assert.Equal("Expected xs to be null or empty, but found 2 items:\n  [0] 1\n  [1] 2", Message(() => xs.Should().BeNullOrEmpty())); }
    [Fact] public void BeNullOrEmpty_works_on_a_dictionary() { Dictionary<string, int>? none = null; none.Should().BeNullOrEmpty(); new Dictionary<string, int>().Should().BeNullOrEmpty(); Assert.Throws<AverException>(() => new Dictionary<string, int> { ["a"] = 1 }.Should().BeNullOrEmpty()); }

    [Fact] public void BeInAscendingOrder_by_comparer_passes() { new[] { "a", "B", "c" }.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase); }
    [Fact] public void BeInAscendingOrder_by_comparer_uses_the_comparer() { var xs = new[] { "a", "B", "c" }; Assert.Throws<AverException>(() => xs.Should().BeInAscendingOrder(StringComparer.Ordinal)); }
    [Fact] public void BeInAscendingOrder_by_comparer_message() { var xs = new[] { "a", "B", "c" }; Assert.Equal("Expected xs to be in ascending order, but [1] \"B\" comes after [0] \"a\".", Message(() => xs.Should().BeInAscendingOrder(StringComparer.Ordinal))); }
    [Fact] public void BeInAscendingOrder_by_comparer_chains() { new[] { "a", "b" }.Should().BeInAscendingOrder(StringComparer.Ordinal).And.OnlyHaveUniqueItems(); }
    [Fact] public void BeInAscendingOrder_by_comparer_null_throws_ArgumentNullException() { Assert.Throws<ArgumentNullException>(() => new[] { "a" }.Should().BeInAscendingOrder((IComparer<string>)null!)); }
    [Fact] public void BeInDescendingOrder_by_comparer_passes_and_fails() { new[] { "c", "B", "a" }.Should().BeInDescendingOrder(StringComparer.OrdinalIgnoreCase); var xs = new[] { "a", "b" }; Assert.Equal("Expected xs to be in descending order, but [1] \"b\" comes after [0] \"a\".", Message(() => xs.Should().BeInDescendingOrder(StringComparer.Ordinal))); }
    [Fact] public void BeInDescendingOrder_by_comparer_null_throws_ArgumentNullException() { Assert.Throws<ArgumentNullException>(() => new[] { "a" }.Should().BeInDescendingOrder((IComparer<string>)null!)); }
}
