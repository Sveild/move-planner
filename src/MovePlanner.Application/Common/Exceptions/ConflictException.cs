namespace MovePlanner.Application.Common.Exceptions;

/// <summary>
/// Signals that a request conflicts with the current state of a resource, such as creating a
/// duplicate. Handlers throw it for an expected failure instead of returning a failure value, and
/// the API maps it to a 409 response.
/// </summary>
public sealed class ConflictException : Exception
{
    private const string DefaultMessage = "The request conflicts with the current state of the resource.";

    /// <summary>Creates the exception with a generic message.</summary>
    public ConflictException()
        : base(DefaultMessage)
    {
    }

    /// <summary>Creates the exception with a description of the conflict.</summary>
    /// <param name="message">Description of the conflict.</param>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description of the conflict and its cause.</summary>
    /// <param name="message">Description of the conflict.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
