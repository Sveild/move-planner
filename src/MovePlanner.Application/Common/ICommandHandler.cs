namespace MovePlanner.Application.Common;

/// <summary>
/// Handles a command and returns its outcome. Implementations are registered automatically, so a
/// new handler needs no manual wiring.
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
///     public Task&lt;Result&lt;ApplicationId&gt;&gt; HandleAsync(
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
    /// <returns>The outcome of the command.</returns>
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
