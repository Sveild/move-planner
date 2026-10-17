namespace MovePlanner.Application.Common;

/// <summary>
/// Handles a query and returns its outcome. Implementations are registered automatically, so a
/// new handler needs no manual wiring.
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
///     public Task&lt;Result&lt;JobApplicationDto&gt;&gt; HandleAsync(
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
    /// <returns>The outcome of the query.</returns>
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
