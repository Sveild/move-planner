using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MovePlanner.Infrastructure;
using MovePlanner.Tests.Support;

namespace MovePlanner.Api.IntegrationTests;

/// <summary>
/// Boots the real application (<see cref="Program"/>) against the shared PostgreSQL container, so
/// tests exercise the same composition root the API runs in production. Booting the application
/// instead of faking its dependencies keeps Wolverine's discovery of the Application assembly intact
/// and lets the host provision its own message storage in the <c>wolverine</c> schema while it runs
/// in Development (ADR-0004). The connection string also goes into the process environment, because
/// the application reads its configuration while the host is still being built, before
/// <see cref="ConfigureWebHost"/> runs.
/// </summary>
public sealed class PostgresApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public PostgresApiFactory(PostgresContainerFixture postgres)
    {
        ArgumentNullException.ThrowIfNull(postgres);

        _connectionString = postgres.ConnectionString;
        Environment.SetEnvironmentVariable(AppDbContextFactory.ConnectionStringVariable, _connectionString);
    }

    /// <summary>
    /// Gets the connection string of the database the application under test runs against.
    /// </summary>
    public string ConnectionString => _connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"] = _connectionString,
            }));
    }
}
