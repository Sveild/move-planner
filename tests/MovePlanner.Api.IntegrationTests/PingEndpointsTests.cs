using System.Net;
using FluentAssertions;
using MovePlanner.Api.Endpoints.Diagnostics;
using MovePlanner.Application.Diagnostics;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class PingEndpointsTests : IClassFixture<PostgresApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public PingEndpointsTests(PostgresApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task The_ping_endpoint_echoes_the_message_from_the_query_string()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/ping?message=pong", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Be("pong");
    }

    [Fact]
    public async Task The_ping_endpoint_rejects_a_null_bus()
    {
        Func<Task> ping = () => PingEndpoints.Ping(
            new PingQuery("pong"),
            null!,
            TestContext.Current.CancellationToken);

        await ping.Should().ThrowAsync<ArgumentNullException>().WithParameterName("bus");
    }
}
