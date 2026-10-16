using Npgsql;
using Xunit;

namespace MovePlanner.Tests.Support;

/// <summary>
/// Creates a fresh, uniquely named database inside the shared PostgreSQL container for a
/// single test class and drops it again afterwards. Use it as an <c>IClassFixture</c> on a
/// class that also belongs to the Postgres collection.
/// </summary>
public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgresContainerFixture _container;
    private readonly string _databaseName = $"test_{Guid.NewGuid():N}";
    private string? _connectionString;

    public PostgresDatabaseFixture(PostgresContainerFixture container)
    {
        _container = container;
    }

    /// <summary>
    /// Gets the connection string that points at the isolated database owned by the test class.
    /// </summary>
    public string ConnectionString => _connectionString
        ?? throw new InvalidOperationException("The test database has not been created yet.");

    public async ValueTask InitializeAsync()
    {
        await using (var connection = new NpgsqlConnection(_container.ConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
#pragma warning disable CA2100 // CREATE DATABASE cannot be parameterized; the name is a generated GUID, not user input.
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
#pragma warning restore CA2100
            await command.ExecuteNonQueryAsync();
        }

        _connectionString = new NpgsqlConnectionStringBuilder(_container.ConnectionString)
        {
            Database = _databaseName,
        }.ConnectionString;
    }

    public async ValueTask DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(_container.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
#pragma warning disable CA2100 // See InitializeAsync.
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
#pragma warning restore CA2100
        await command.ExecuteNonQueryAsync();
    }
}
