namespace MovePlanner.Domain.Exceptions;

/// <summary>
/// Signals that an operation violates a rule of the domain, such as acting on an entity in a state
/// that does not allow it. Handlers throw it for an expected rule violation instead of returning a
/// failure value, and the API maps it to a 400 response.
/// </summary>
public class DomainException : Exception
{
    private const string DefaultMessage = "A rule of the domain was violated.";

    /// <summary>Creates the exception with a generic description of a violated rule.</summary>
    public DomainException()
        : base(DefaultMessage)
    {
    }

    /// <summary>Creates the exception with a description of the rule that was violated.</summary>
    /// <param name="message">Description of the violated rule.</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description of the violated rule and its cause.</summary>
    /// <param name="message">Description of the violated rule.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
