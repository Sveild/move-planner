using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MovePlanner.Application.Diagnostics;
using Wolverine.Tracking;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

/// <summary>
/// Covers Wolverine's tracked sessions, the helper asynchronous tests use to wait for handlers and
/// cascading dispatch instead of sleeping for a fixed amount of time.
/// </summary>
[Collection(PostgresCollectionDefinition.Name)]
public sealed class TrackedSessionTests : IClassFixture<PostgresApiFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    public TrackedSessionTests(PostgresApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _scope = factory.Services.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    [Fact]
    public async Task A_tracked_session_waits_for_the_handler_to_finish()
    {
        ITrackedSession session = await _scope.ServiceProvider
            .TrackActivity()
            .Timeout(TimeSpan.FromSeconds(30))
            .InvokeMessageAndWaitAsync(new PingQuery("tracked"));

        session.Executed.MessagesOf<PingQuery>().Should().ContainSingle();
        session.MessageFailed.MessagesOf<PingQuery>().Should().BeEmpty();
        session.AllExceptions().Should().BeEmpty();
    }
}
