namespace MovePlanner.Application.Common;

/// <summary>
/// Classifies an <see cref="Error"/> so callers can react to it and the API layer can map it
/// to a response.
/// </summary>
public enum ErrorKind
{
    /// <summary>The request was malformed or violated a rule. Maps to a 400 response.</summary>
    Validation,

    /// <summary>The requested resource does not exist. Maps to a 404 response.</summary>
    NotFound,

    /// <summary>The request conflicts with the current state. Maps to a 409 response.</summary>
    Conflict,

    /// <summary>An unexpected failure. Maps to a 500 response.</summary>
    Unexpected,
}
