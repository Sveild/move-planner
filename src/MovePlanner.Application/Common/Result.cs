namespace MovePlanner.Application.Common;

/// <summary>
/// Creates <see cref="Result{T}"/> instances. A static factory keeps the type argument out of
/// the call site where it can be inferred, for example <c>Result.Success(id)</c>.
/// </summary>
public static class Result
{
    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <typeparam name="T">The type of the value produced on success.</typeparam>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success<T>(T value) => new(value);

    /// <summary>Creates a failed result carrying <paramref name="error"/>.</summary>
    /// <typeparam name="T">The type of the value the operation would have produced.</typeparam>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure<T>(Error error) => new(error);
}

/// <summary>
/// The outcome of an operation that either carries a value or an <see cref="Error"/>.
/// </summary>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;

    internal Result(T value) => _value = value;

    internal Result(Error error) => Error = error;

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error when the operation failed; <see langword="null"/> on success.</summary>
    public Error? Error { get; }

    /// <summary>Gets the value when the operation succeeded.</summary>
    /// <exception cref="InvalidOperationException">The operation failed.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result does not carry a value.");
}
