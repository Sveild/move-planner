using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MovePlanner.Api.Endpoints.Diagnostics;
using MovePlanner.Application.Diagnostics;
using MovePlanner.Infrastructure;
using MovePlanner.Tests.Support;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class PingEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PingEndpointsTests(WebApplicationFactory<Program> factory, PostgresContainerFixture postgres)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(postgres);

        Environment.SetEnvironmentVariable(AppDbContextFactory.ConnectionStringVariable, postgres.ConnectionString);
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>(
                StringComparer.Ordinal)
            {
                [$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"] =
                    postgres.ConnectionString,
            })));
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
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
