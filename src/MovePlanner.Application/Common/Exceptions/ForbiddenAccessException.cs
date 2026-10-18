namespace MovePlanner.Application.Common.Exceptions;

/// <summary>
/// Signals that the current user is authenticated but not allowed to act on the resource. Handlers
/// throw it for an expected failure instead of returning a failure value, and the API maps it to a
/// 403 response. A missing authentication is a <see cref="UnauthorizedAccessException"/> instead.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    private const string DefaultMessage = "The current user is not allowed to access this resource.";

    /// <summary>Creates the exception with a generic message.</summary>
    public ForbiddenAccessException()
        : base(DefaultMessage)
    {
    }

    /// <summary>Creates the exception with a description of the denied access.</summary>
    /// <param name="message">Description of the denied access.</param>
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description of the denied access and its cause.</summary>
    /// <param name="message">Description of the denied access.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
