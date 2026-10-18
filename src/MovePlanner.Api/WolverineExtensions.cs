using MovePlanner.Application.Diagnostics;
using MovePlanner.Infrastructure;
using Wolverine;

namespace MovePlanner.Api;

/// <summary>Configures Wolverine for the host.</summary>
public static class WolverineExtensions
{
    /// <summary>
    /// Adds Wolverine to <paramref name="builder"/>, points its handler discovery at the
    /// Application assembly, where the command and query handlers live (Wolverine would otherwise
    /// only scan the entry assembly), and stores its durable message state in the application database.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="configuration">The application configuration to read the connection string from.</param>
    /// <param name="environment">The host environment, which decides whether Wolverine may manage its own tables.</param>
    /// <returns>The same <paramref name="builder"/> instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/>, <paramref name="configuration"/> or <paramref name="environment"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">The database connection string is not configured.</exception>
    public static IHostBuilder UseMovePlannerWolverine(
        this IHostBuilder builder,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        return builder.UseWolverine(options =>
        {
            options.ApplicationAssembly = typeof(Program).Assembly;
            options.Discovery.IncludeAssembly(typeof(PingQueryHandler).Assembly);
            options.PersistWithPostgresql(
                configuration[$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"],
                environment.IsDevelopment());
        });
    }
}
