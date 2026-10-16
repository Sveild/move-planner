using Testcontainers.PostgreSql;
using Xunit;

namespace MovePlanner.Tests.Support;

/// <summary>
/// Starts a single PostgreSQL container for the whole test run. Reference it from an
/// <c>[CollectionDefinition]</c> in a test project to share the container across that assembly.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    /// <summary>
    /// Gets the connection string that points at the default database of the running container.
    /// </summary>
    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("The PostgreSQL container has not been started yet.");

    public async ValueTask InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
