using System.Data;
using System.Net;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class ApiSmokeTests : IClassFixture<PostgresApiFactory>, IDisposable
{
    private readonly PostgresApiFactory _factory;
    private readonly HttpClient _client;

    public ApiSmokeTests(PostgresApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task Unknown_route_returns_not_found()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/does-not-exist", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Shared_container_exposes_a_running_postgres()
    {
        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        connection.State.Should().Be(ConnectionState.Open);
    }
}
