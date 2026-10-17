namespace MovePlanner.Application.Common;

/// <summary>
/// Describes why an operation failed. Errors are returned as values rather than thrown, so an
/// expected failure is part of a handler's contract instead of an exception.
/// </summary>
/// <param name="Kind">Category of the error, used to choose a response.</param>
/// <param name="Code">Stable, machine-readable identifier, for example <c>job_application.not_found</c>.</param>
/// <param name="Message">Human-readable description of what went wrong.</param>
public sealed record Error(ErrorKind Kind, string Code, string Message)
{
    /// <summary>Creates a <see cref="ErrorKind.Validation"/> error.</summary>
    /// <param name="code">Stable, machine-readable identifier.</param>
    /// <param name="message">Human-readable description of what went wrong.</param>
    /// <returns>The new error.</returns>
    public static Error Validation(string code, string message) =>
        new(ErrorKind.Validation, code, message);

    /// <summary>Creates a <see cref="ErrorKind.NotFound"/> error.</summary>
    /// <param name="code">Stable, machine-readable identifier.</param>
    /// <param name="message">Human-readable description of what went wrong.</param>
    /// <returns>The new error.</returns>
    public static Error NotFound(string code, string message) =>
        new(ErrorKind.NotFound, code, message);

    /// <summary>Creates a <see cref="ErrorKind.Conflict"/> error.</summary>
    /// <param name="code">Stable, machine-readable identifier.</param>
    /// <param name="message">Human-readable description of what went wrong.</param>
    /// <returns>The new error.</returns>
    public static Error Conflict(string code, string message) =>
        new(ErrorKind.Conflict, code, message);

    /// <summary>Creates a <see cref="ErrorKind.Unexpected"/> error.</summary>
    /// <param name="code">Stable, machine-readable identifier.</param>
    /// <param name="message">Human-readable description of what went wrong.</param>
    /// <returns>The new error.</returns>
    public static Error Unexpected(string code, string message) =>
        new(ErrorKind.Unexpected, code, message);
}
