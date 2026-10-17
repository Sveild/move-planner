using MovePlanner.Application.Common;

namespace MovePlanner.Api.Endpoints;

/// <summary>Maps the results of the Application layer onto HTTP responses.</summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps <paramref name="result"/> onto an <see cref="IResult"/>: a success becomes a 200
    /// response carrying the value, a failure becomes a problem response that describes the error.
    /// </summary>
    /// <typeparam name="T">The type of the value produced on success.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <returns>The HTTP response the result maps to.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error!);
    }

    private static IResult ToProblem(Error error)
    {
        (int statusCode, string title) = error.Kind switch
        {
            ErrorKind.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorKind.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorKind.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorKind.Unexpected => (StatusCodes.Status500InternalServerError, "Unexpected error"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Message,
            extensions: new Dictionary<string, object?>(StringComparer.Ordinal) { ["code"] = error.Code });
    }
}
