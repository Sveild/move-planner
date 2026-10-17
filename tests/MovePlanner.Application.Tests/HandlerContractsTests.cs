using FluentAssertions;
using MovePlanner.Application.Common;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class HandlerContractsTests
{
    [Fact]
    public async Task A_command_handler_returns_its_response()
    {
        ICommandHandler<Increment, int> handler = new IncrementHandler();

        Result<int> result = await handler.HandleAsync(
            new Increment(41),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task A_query_handler_returns_its_response()
    {
        IQueryHandler<Echo, string> handler = new EchoHandler();

        Result<string> result = await handler.HandleAsync(
            new Echo("hello"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public async Task A_command_handler_can_produce_no_value_using_unit()
    {
        ICommandHandler<MarkSeen, Unit> handler = new MarkSeenHandler();

        Result<Unit> result = await handler.HandleAsync(
            new MarkSeen(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(default(Unit));
    }

    private sealed record Increment(int Value);

    private sealed class IncrementHandler : ICommandHandler<Increment, int>
    {
        public Task<Result<int>> HandleAsync(Increment command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Result.Success(command.Value + 1));
        }
    }

    private sealed record Echo(string Value);

    private sealed class EchoHandler : IQueryHandler<Echo, string>
    {
        public Task<Result<string>> HandleAsync(Echo query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Result.Success(query.Value));
        }
    }

    private sealed record MarkSeen(Guid Id);

    private sealed class MarkSeenHandler : ICommandHandler<MarkSeen, Unit>
    {
        public Task<Result<Unit>> HandleAsync(MarkSeen command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Result.Success(default(Unit)));
        }
    }
}
