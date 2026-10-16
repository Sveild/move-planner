using FluentAssertions;
using MovePlanner.Tests.Support;
using Npgsql;
using Xunit;

namespace MovePlanner.Infrastructure.Tests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class IsolatedDatabaseTests : IClassFixture<PostgresDatabaseFixture>
{
    private readonly PostgresDatabaseFixture _database;

    public IsolatedDatabaseTests(PostgresDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Class_fixture_creates_an_isolated_database()
    {
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        connection.Database.Should().NotBe("postgres");
    }
}
