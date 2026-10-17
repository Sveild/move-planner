namespace MovePlanner.Api.Endpoints.JobApplications;

/// <summary>Maps the endpoints of the job applications feature.</summary>
public static class JobApplicationsEndpoints
{
    private const string RoutePrefix = "/job-applications";

    /// <summary>Maps the job application endpoints onto <paramref name="endpoints"/>.</summary>
    /// <param name="endpoints">The route builder to map the endpoints onto.</param>
    /// <returns>The route group of the feature, so the handlers can be mapped onto it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="endpoints"/> is <see langword="null"/>.</exception>
    public static RouteGroupBuilder MapJobApplications(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapGroup(RoutePrefix);
    }
}
