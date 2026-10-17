using MovePlanner.Api.Endpoints.JobApplications;

namespace MovePlanner.Api.Endpoints;

/// <summary>Maps the endpoint groups of every feature onto the application.</summary>
public static class EndpointsExtensions
{
    /// <summary>
    /// Maps the endpoints of every feature onto <paramref name="app"/>. Each feature keeps its own
    /// endpoint group, so this is the only place that knows about all of them.
    /// </summary>
    /// <param name="app">The application to map the endpoints onto.</param>
    /// <returns>The same <paramref name="app"/> instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapJobApplications();

        return app;
    }
}
