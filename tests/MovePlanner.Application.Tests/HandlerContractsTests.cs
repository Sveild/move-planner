using FluentAssertions;
using MovePlanner.Application.Common;
using MovePlanner.Application.Common.Exceptions;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class HandlerContractsTests
{
    [Fact]
    public async Task A_command_handler_returns_its_response()
    {
        ICommandHandler<Increment, int> handler = new IncrementHandler();

        int response = await handler.HandleAsync(
            new Increment(41),
            TestContext.Current.CancellationToken);

        response.Should().Be(42);
    }

    [Fact]
    public async Task A_query_handler_returns_its_response()
    {
        IQueryHandler<Echo, string> handler = new EchoHandler();

        string response = await handler.HandleAsync(
            new Echo("hello"),
            TestContext.Current.CancellationToken);

        response.Should().Be("hello");
    }

    [Fact]
    public async Task A_command_handler_can_produce_no_value_using_unit()
    {
        ICommandHandler<MarkSeen, Unit> handler = new MarkSeenHandler();

        Unit response = await handler.HandleAsync(
            new MarkSeen(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        response.Should().Be(default(Unit));
    }

    [Fact]
    public async Task A_handler_signals_an_expected_failure_by_throwing()
    {
        IQueryHandler<Echo, string> handler = new EchoHandler();

        Func<Task> handle = () => handler.HandleAsync(
            new Echo("missing"),
            TestContext.Current.CancellationToken);

        await handle.Should().ThrowAsync<NotFoundException>();
    }

    private sealed record Increment(int Value);

    private sealed class IncrementHandler : ICommandHandler<Increment, int>
    {
        public Task<int> HandleAsync(Increment command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(command.Value + 1);
        }
    }

    private sealed record Echo(string Value);

    private sealed class EchoHandler : IQueryHandler<Echo, string>
    {
        public Task<string> HandleAsync(Echo query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(query.Value, "missing", StringComparison.Ordinal))
            {
                throw new NotFoundException("The echo is missing.");
            }

            return Task.FromResult(query.Value);
        }
    }

    private sealed record MarkSeen(Guid Id);

    private sealed class MarkSeenHandler : ICommandHandler<MarkSeen, Unit>
    {
        public Task<Unit> HandleAsync(MarkSeen command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(default(Unit));
        }
    }
}
