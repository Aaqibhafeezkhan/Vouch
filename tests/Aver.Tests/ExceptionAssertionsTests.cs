using Xunit;

namespace Aver.Tests;

public class ExceptionAssertionsTests
{
    private sealed class NoStackException() : Exception("m")
    {
        public override string? StackTrace => null;
    }

    private static string Msg(Action a) => Assert.Throws<AverException>(a).Message;

    private static async Task<string> MsgAsync(Func<Task> a) => (await Assert.ThrowsAsync<AverException>(a)).Message;

    [Fact]
    public void Throw_passes()
    {
        Action act = () => throw new InvalidOperationException("x");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Throw_accepts_derived()
    {
        Action act = () => throw new ArgumentNullException("p");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throw_fails_when_nothing_is_thrown_with_exact_message()
    {
        Action act = () => { };
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_fails_on_wrong_type_with_exact_message()
    {
        Action act = () => throw new ArgumentException("Name is required.");
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw ArgumentException:\n  \"Name is required.\"",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_fails_on_null_action_with_exact_message()
    {
        Action? act = null;
        Assert.Equal("Expected act to throw Exception, but found null.", Msg(() => act.Should().Throw<Exception>()));
    }

    [Fact]
    public void Throw_does_not_mistake_an_AverException_for_the_expected_type()
    {
        Action act = () => throw new AverException("inner");
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw AverException:\n  \"inner\"",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_can_expect_an_AverException()
    {
        Action act = () => throw new AverException("inner");
        act.Should().Throw<AverException>().WithMessage("inner");
    }

    [Fact]
    public void Throw_does_not_unwrap_AggregateException()
    {
        Action act = () => throw new AggregateException(new InvalidOperationException("x"));
        Assert.StartsWith("Expected act to throw InvalidOperationException, but it threw AggregateException:",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Which_is_the_exception()
    {
        Action act = () => throw new ArgumentException("m", "p");
        Assert.Equal("p", act.Should().Throw<ArgumentException>().Which.ParamName);
    }

    [Fact]
    public void Subject_and_And_return_the_exception_assertions()
    {
        Action act = () => throw new ArgumentException("m");
        var result = act.Should().Throw<ArgumentException>();
        Assert.Same(result.Which, result.Subject);
        Assert.Same(result, result.And);
    }

    [Fact]
    public void Func_Throw_passes()
    {
        Func<int> act = () => throw new InvalidOperationException();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Func_Throw_fails_with_exact_message()
    {
        Func<int> act = () => 1;
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Func_Throw_fails_on_null()
    {
        Func<int>? act = null;
        Assert.Equal("Expected act to throw Exception, but found null.", Msg(() => act.Should().Throw<Exception>()));
    }

    [Fact]
    public void Func_NotThrow_passes_and_fails()
    {
        Func<int> ok = () => 1;
        ok.Should().NotThrow();
        Func<int> bad = () => throw new ArgumentException("boom");
        Assert.StartsWith("Expected bad not to throw, but it threw ArgumentException:\n  \"boom\"", Msg(() => bad.Should().NotThrow()));
    }

    [Fact]
    public void NotThrow_passes()
    {
        Action act = () => { };
        act.Should().NotThrow().And.NotThrow();
    }

    [Fact]
    public void NotThrow_fails_with_exact_message()
    {
        Action act = () => throw new ArgumentException("Name is required.");
        var message = Msg(() => act.Should().NotThrow());
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException:\n  \"Name is required.\"\n  at ", message);
        Assert.Equal(3, message.Split('\n').Length);
    }

    [Fact]
    public async Task NotThrowAsync_omits_the_stack_line_when_the_exception_has_none()
    {
        Func<Task> act = () => throw new NoStackException();
        Assert.Equal("Expected act not to throw, but it threw NoStackException:\n  \"m\"",
            await MsgAsync(() => act.Should().NotThrowAsync()));
    }

    [Fact]
    public void NotThrow_fails_on_null_action()
    {
        Action? act = null;
        Assert.Equal("Expected act not to throw, but found null.", Msg(() => act.Should().NotThrow()));
    }

    [Fact]
    public void WithMessage_passes_with_wildcards_and_is_case_sensitive()
    {
        Action act = () => throw new InvalidOperationException("Name is required.");
        act.Should().Throw<InvalidOperationException>().WithMessage("Name * required.");
        Assert.NotNull(Msg(() => act.Should().Throw<InvalidOperationException>().WithMessage("name*")));
    }

    [Fact]
    public void WithMessage_fails_with_exact_message()
    {
        Action act = () => throw new InvalidOperationException("Branch code is missing.");
        Assert.Equal("Expected act's exception message to match \"Name * required.\", but found \"Branch code is missing.\".",
            Msg(() => act.Should().Throw<InvalidOperationException>().WithMessage("Name * required.")));
    }

    [Fact]
    public void WithMessage_null_pattern_throws_ArgumentNullException()
    {
        Action act = () => throw new InvalidOperationException("x");
        var assertions = act.Should().Throw<InvalidOperationException>();
        Assert.Throws<ArgumentNullException>(() => assertions.WithMessage(null!));
    }

    [Fact]
    public async Task ThrowAsync_passes()
    {
        Func<Task> act = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_then_WithMessage_on_the_task()
    {
        Func<Task> act = () => throw new InvalidOperationException("Name is required.");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Name*required.");
    }

    [Fact]
    public async Task ThrowAsync_awaited_then_WithMessage()
    {
        Func<Task> act = async () => { await Task.Yield(); throw new ArgumentException("x"); };
        (await act.Should().ThrowAsync<ArgumentException>()).WithMessage("x");
    }

    [Fact]
    public async Task WithMessage_on_the_task_fails_with_exact_message()
    {
        Func<Task> act = () => Task.FromException(new InvalidOperationException("Branch code is missing."));
        Assert.Equal("Expected act's exception message to match \"Name * required.\", but found \"Branch code is missing.\".",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Name * required.")));
    }

    [Fact]
    public async Task WithMessage_on_the_task_propagates_the_throw_failure()
    {
        Func<Task> act = () => Task.CompletedTask;
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>().WithMessage("x")));
    }

    [Fact]
    public async Task WithMessage_on_the_task_null_pattern_throws_ArgumentNullException()
    {
        Func<Task> act = () => throw new InvalidOperationException("x");
        await Assert.ThrowsAsync<ArgumentNullException>(() => act.Should().ThrowAsync<InvalidOperationException>().WithMessage(null!));
    }

    [Fact]
    public async Task ThrowAsync_accepts_derived()
    {
        Func<Task> act = () => Task.FromException(new ArgumentNullException("p"));
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Sync_throw_counts_for_ThrowAsync()
    {
        Func<Task> act = () => throw new InvalidOperationException();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_fails_when_nothing_is_thrown()
    {
        Func<Task> act = () => Task.CompletedTask;
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task Null_task_fails_with_message()
    {
        Func<Task> act = () => null!;
        Assert.Equal("Expected act to throw Exception, but it returned a null Task.",
            await MsgAsync(() => act.Should().ThrowAsync<Exception>()));
    }

    [Fact]
    public async Task Null_delegate_fails_with_message()
    {
        Func<Task>? act = null;
        Assert.Equal("Expected act to throw Exception, but found null.",
            await MsgAsync(() => act.Should().ThrowAsync<Exception>()));
    }

    [Fact]
    public async Task Func_of_Task_of_T_works()
    {
        Func<Task<int>> act = () => throw new InvalidOperationException();
        await act.Should().ThrowAsync<InvalidOperationException>();
        Func<Task<int>> async = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        await async.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Func_of_Task_of_T_null_task_and_null_delegate_fail()
    {
        Func<Task<int>> act = () => null!;
        Assert.Equal("Expected act to throw Exception, but it returned a null Task.",
            await MsgAsync(() => act.Should().ThrowAsync<Exception>()));
        Func<Task<int>>? none = null;
        Assert.Equal("Expected none to throw Exception, but found null.",
            await MsgAsync(() => none.Should().ThrowAsync<Exception>()));
    }

    [Fact]
    public async Task Wrong_type_message()
    {
        Func<Task> act = () => throw new ArgumentException("Name is required.");
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw ArgumentException:\n  \"Name is required.\"",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task Async_AverException_is_not_mistaken_for_the_expected_type()
    {
        Func<Task> act = async () => { await Task.Yield(); throw new AverException("inner"); };
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw AverException:\n  \"inner\"",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task Canceled_task_counts_as_thrown()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Func<Task> act = () => Task.FromCanceled(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ThrowAsync_does_not_unwrap_AggregateException()
    {
        Func<Task> act = () => throw new AggregateException(new InvalidOperationException("x"));
        Assert.StartsWith("Expected act to throw InvalidOperationException, but it threw AggregateException:",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task ThrowAsync_does_not_deadlock_on_a_single_threaded_context()
    {
        var previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        try
        {
            Func<Task> act = async () => { await Task.Delay(10); throw new InvalidOperationException(); };
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    [Fact]
    public async Task NotThrowAsync_passes()
    {
        Func<Task> act = async () => await Task.Yield();
        await act.Should().NotThrowAsync();
        Func<Task<int>> typed = () => Task.FromResult(1);
        await typed.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotThrowAsync_fails_with_exact_message()
    {
        Func<Task> act = () => Task.FromException(new ArgumentException("Name is required."));
        var message = await MsgAsync(() => act.Should().NotThrowAsync());
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException:\n  \"Name is required.\"", message);
    }

    [Fact]
    public async Task NotThrowAsync_fails_on_sync_throw()
    {
        Func<Task> act = () => throw new ArgumentException("m");
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException:\n  \"m\"", await MsgAsync(() => act.Should().NotThrowAsync()));
    }

    [Fact]
    public async Task NotThrowAsync_fails_on_null_delegate_and_null_task()
    {
        Func<Task>? none = null;
        Assert.Equal("Expected none not to throw, but found null.", await MsgAsync(() => none.Should().NotThrowAsync()));
        Func<Task> act = () => null!;
        Assert.Equal("Expected act not to throw, but it returned a null Task.", await MsgAsync(() => act.Should().NotThrowAsync()));
    }
}
