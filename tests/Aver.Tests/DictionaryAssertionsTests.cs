using Xunit;

namespace Aver.Tests;

public class DictionaryAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<AverException>(act).Message;

    private static Dictionary<string, int> Sample() => new() { ["a"] = 1, ["b"] = 2 };

    [Fact] public void Dictionary_resolves() => Assert.IsType<DictionaryAssertions<string, int>>(Sample().Should());
    [Fact] public void IDictionary_resolves() { IDictionary<string, int> d = Sample(); Assert.IsType<DictionaryAssertions<string, int>>(d.Should()); }
    [Fact] public void IReadOnlyDictionary_resolves() { IReadOnlyDictionary<string, int> d = Sample(); Assert.IsType<DictionaryAssertions<string, int>>(d.Should()); }

    [Fact] public void ContainKey_passes() => Sample().Should().ContainKey("a");
    [Fact] public void ContainKey_fails() => Assert.Throws<AverException>(() => Sample().Should().ContainKey("z"));
    [Fact] public void ContainKey_message() { var d = Sample(); Assert.Equal("Expected d to contain key \"z\", but it did not. It has:\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2", Message(() => d.Should().ContainKey("z"))); }
    [Fact] public void ContainKey_Which_is_the_value() => Assert.Equal(2, new Dictionary<string, int> { ["a"] = 2 }.Should().ContainKey("a").Which);
    [Fact] public void ContainKey_works_on_IDictionary_and_IReadOnlyDictionary() { IDictionary<string, int> d = Sample(); IReadOnlyDictionary<string, int> r = Sample(); Assert.Equal(2, d.Should().ContainKey("b").Which); Assert.Equal(1, r.Should().ContainKey("a").Which); Assert.Throws<AverException>(() => r.Should().ContainKey("z")); }
    [Fact] public void ContainKey_null_key_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainKey(null!));
    [Fact] public void ContainKey_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to contain key \"a\", but found null.", Message(() => d.Should().ContainKey("a"))); }

    [Fact] public void NotContainKey_passes() => Sample().Should().NotContainKey("z");
    [Fact] public void NotContainKey_fails() => Assert.Throws<AverException>(() => Sample().Should().NotContainKey("a"));
    [Fact] public void NotContainKey_message() { var d = Sample(); Assert.Equal("Expected d not to contain key \"a\", but found it with value 1.", Message(() => d.Should().NotContainKey("a"))); }
    [Fact] public void NotContainKey_null_key_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotContainKey(null!));

    [Fact] public void ContainKeys_passes() => Sample().Should().ContainKeys("a", "b");
    [Fact] public void ContainKeys_fails() => Assert.Throws<AverException>(() => Sample().Should().ContainKeys("a", "z"));
    [Fact] public void ContainKeys_message() { var d = Sample(); Assert.Equal("Expected d to contain keys [\"a\", \"y\", \"z\"], but it is missing [\"y\", \"z\"]. It has:\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2", Message(() => d.Should().ContainKeys("a", "y", "z"))); }
    [Fact] public void ContainKeys_null_array_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainKeys(null!));
    [Fact] public void ContainKeys_null_element_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainKeys("a", null!));

    [Fact] public void Dictionary_gets_collection_methods_too() => new Dictionary<string, int>().Should().BeEmpty();
    [Fact] public void Dictionary_HaveCount() => Sample().Should().HaveCount(2).And.ContainKey("a");
    [Fact] public void Dictionary_BeEmpty_message() { var d = Sample(); Assert.Equal("Expected d to be empty, but found 2 items:\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2", Message(() => d.Should().BeEmpty())); }
    [Fact] public void Dictionary_Contain_pair() => Sample().Should().Contain(new KeyValuePair<string, int>("a", 1));
    [Fact] public void Dictionary_Contain_predicate_Which_is_the_pair() => Assert.Equal("b", Sample().Should().Contain(kv => kv.Value == 2).Which.Key);
    [Fact] public void Dictionary_null_BeNull_passes() { IDictionary<string, int>? d = null; d.Should().BeNull(); }
    [Fact] public void Dictionary_null_HaveCount_fails() { IReadOnlyDictionary<string, int>? d = null; Assert.Equal("Expected d to contain 1 item, but found null.", Message(() => d.Should().HaveCount(1))); }
}
