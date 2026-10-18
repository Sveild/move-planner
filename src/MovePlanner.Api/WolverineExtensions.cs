using MovePlanner.Application.Diagnostics;
using Wolverine;

namespace MovePlanner.Api;

/// <summary>Configures Wolverine for the host.</summary>
public static class WolverineExtensions
{
    /// <summary>
    /// Adds Wolverine to <paramref name="builder"/> and points its handler discovery at the
    /// Application assembly, where the command and query handlers live. Wolverine would otherwise
    /// only scan the entry assembly.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static IHostBuilder UseMovePlannerWolverine(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.UseWolverine(options =>
        {
            options.ApplicationAssembly = typeof(Program).Assembly;
            options.Discovery.IncludeAssembly(typeof(PingQueryHandler).Assembly);
        });
    }
}
