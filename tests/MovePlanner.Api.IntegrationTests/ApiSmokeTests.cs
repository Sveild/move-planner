using System.Data;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MovePlanner.Tests.Support;
using Npgsql;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly PostgresContainerFixture _postgres;

    public ApiSmokeTests(WebApplicationFactory<Program> factory, PostgresContainerFixture postgres)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(postgres);

        _client = factory.CreateClient();
        _postgres = postgres;
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
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        connection.State.Should().Be(ConnectionState.Open);
    }
}
