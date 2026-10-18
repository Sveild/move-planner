namespace MovePlanner.Domain.Exceptions;

/// <summary>
/// Signals an attempt to move an entity to a state that its current state does not allow, such as
/// accepting a job application that was already rejected. It is a <see cref="DomainException"/> so
/// the API maps it to a 409 response: the request was understood but conflicts with the state.
/// </summary>
public sealed class InvalidStatusTransitionException : DomainException
{
    private const string DefaultMessage = "The status transition is not allowed from the current status.";

    /// <summary>Creates the exception with a generic description of a rejected transition.</summary>
    public InvalidStatusTransitionException()
        : base(DefaultMessage)
    {
    }

    /// <summary>Creates the exception with a description of the rejected transition.</summary>
    /// <param name="message">Description of the rejected transition.</param>
    public InvalidStatusTransitionException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description of the rejected transition and its cause.</summary>
    /// <param name="message">Description of the rejected transition.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public InvalidStatusTransitionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
