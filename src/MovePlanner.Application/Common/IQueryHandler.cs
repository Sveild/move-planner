namespace MovePlanner.Application.Common;

/// <summary>
/// Handles a query and returns its response. Implementations are registered automatically, so a
/// new handler needs no manual wiring. A handler returns the value of the operation and signals an
/// expected failure by throwing a typed exception instead of returning a failure value.
/// </summary>
/// <typeparam name="TQuery">The query to handle.</typeparam>
/// <typeparam name="TResponse">The type of the value produced on success.</typeparam>
/// <example>
/// <code>
/// public sealed record GetJobApplicationQuery(Guid Id);
///
/// public sealed class GetJobApplicationHandler
///     : IQueryHandler&lt;GetJobApplicationQuery, JobApplicationDto&gt;
/// {
///     public Task&lt;JobApplicationDto&gt; HandleAsync(
///         GetJobApplicationQuery query,
///         CancellationToken cancellationToken) =&gt; ...;
/// }
/// </code>
/// </example>
public interface IQueryHandler<TQuery, TResponse>
{
    /// <summary>Handles <paramref name="query"/>.</summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The value produced by the query.</returns>
    /// <exception cref="Exceptions.NotFoundException">The query targets a resource that does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenAccessException">The current user may not read the resource.</exception>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
