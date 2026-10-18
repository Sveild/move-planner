namespace MovePlanner.Application.Common.Exceptions;

/// <summary>
/// Signals that a requested resource does not exist. Handlers throw it for an expected failure
/// instead of returning a failure value, and the API maps it to a 404 response.
/// </summary>
public sealed class NotFoundException : Exception
{
    private const string DefaultMessage = "The requested resource was not found.";

    /// <summary>Creates the exception with a generic message.</summary>
    public NotFoundException()
        : base(DefaultMessage)
    {
    }

    /// <summary>Creates the exception with a description of what was not found.</summary>
    /// <param name="message">Description of what was not found.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description of what was not found and its cause.</summary>
    /// <param name="message">Description of what was not found.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
