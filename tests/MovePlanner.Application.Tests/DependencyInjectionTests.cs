using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MovePlanner.Application.Common;
using Xunit;

namespace MovePlanner.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_returns_the_same_service_collection()
    {
        var services = new ServiceCollection();

        services.AddApplication().Should().BeSameAs(services);
    }

    [Fact]
    public async Task Command_handlers_are_discovered_and_resolved()
    {
        var services = new ServiceCollection();
        services.AddHandlers(typeof(DependencyInjectionTests).Assembly);

        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        ICommandHandler<Increment, int> handler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<Increment, int>>();

        handler.Should().BeOfType<IncrementHandler>();

        Result<int> result = await handler.HandleAsync(new Increment(41), TestContext.Current.CancellationToken);

        result.Value.Should().Be(42);
    }

    [Fact]
    public void Query_handlers_are_discovered_and_resolved()
    {
        var services = new ServiceCollection();
        services.AddHandlers(typeof(DependencyInjectionTests).Assembly);

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IQueryHandler<Echo, string> handler =
            scope.ServiceProvider.GetRequiredService<IQueryHandler<Echo, string>>();

        handler.Should().BeOfType<EchoHandler>();
    }

    private sealed record Increment(int Value);

    private sealed class IncrementHandler : ICommandHandler<Increment, int>
    {
        public Task<Result<int>> HandleAsync(Increment command, CancellationToken cancellationToken)
            => Task.FromResult(Result.Success(command.Value + 1));
    }

    private sealed record Echo(string Value);

    private sealed class EchoHandler : IQueryHandler<Echo, string>
    {
        public Task<Result<string>> HandleAsync(Echo query, CancellationToken cancellationToken)
            => Task.FromResult(Result.Success(query.Value));
    }
}
