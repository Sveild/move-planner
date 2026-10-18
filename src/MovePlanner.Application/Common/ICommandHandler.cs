namespace MovePlanner.Application.Common;

/// <summary>
/// Handles a command and returns its response. Implementations are registered automatically, so a
/// new handler needs no manual wiring. A handler returns the value of the operation and signals an
/// expected failure by throwing a typed exception instead of returning a failure value.
/// </summary>
/// <typeparam name="TCommand">The command to handle.</typeparam>
/// <typeparam name="TResponse">The type of the value produced on success.</typeparam>
/// <example>
/// <code>
/// public sealed record CreateJobApplicationCommand(string Company, string Role);
///
/// public sealed class CreateJobApplicationHandler
///     : ICommandHandler&lt;CreateJobApplicationCommand, ApplicationId&gt;
/// {
///     public Task&lt;ApplicationId&gt; HandleAsync(
///         CreateJobApplicationCommand command,
///         CancellationToken cancellationToken) =&gt; ...;
/// }
/// </code>
/// </example>
public interface ICommandHandler<TCommand, TResponse>
{
    /// <summary>Handles <paramref name="command"/>.</summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The value produced by the command.</returns>
    /// <exception cref="Exceptions.NotFoundException">The command targets a resource that does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">The command conflicts with the current state.</exception>
    /// <exception cref="Exceptions.ForbiddenAccessException">The current user may not run the command.</exception>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
