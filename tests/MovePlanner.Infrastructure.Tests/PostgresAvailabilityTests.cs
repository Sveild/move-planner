using System.Data;
using FluentAssertions;
using MovePlanner.Tests.Support;
using Npgsql;
using Xunit;

namespace MovePlanner.Infrastructure.Tests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class PostgresAvailabilityTests
{
    private readonly PostgresContainerFixture _postgres;

    public PostgresAvailabilityTests(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Shared_container_exposes_a_running_postgres()
    {
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        connection.State.Should().Be(ConnectionState.Open);
    }
}
