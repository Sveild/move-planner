using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovePlanner.Application.Common.Exceptions;
using MovePlanner.Domain.Exceptions;

namespace MovePlanner.Api.Errors;

/// <summary>
/// Turns an exception that escapes a handler into a ProblemDetails response, so no endpoint builds
/// an error response itself. An expected failure gets the status of the exception taxonomy; an
/// unexpected one is logged and reported as a 500 without leaking its message.
/// </summary>
public sealed partial class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>Creates the handler with the services it writes the response through.</summary>
    /// <param name="problemDetailsService">Writes the ProblemDetails response.</param>
    /// <param name="logger">Logs an unexpected failure.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="problemDetailsService"/> or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsService);
        ArgumentNullException.ThrowIfNull(logger);

        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// <paramref name="httpContext"/> or <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        (int statusCode, string title) = Map(exception);
        bool unexpected = statusCode == StatusCodes.Status500InternalServerError;

        if (unexpected)
        {
            LogUnhandledException(_logger, httpContext.Request.Method, httpContext.Request.Path, exception);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = unexpected ? null : exception.Message,
            },
        });
    }

    private static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        // InvalidStatusTransitionException derives from DomainException, so it is matched first.
        InvalidStatusTransitionException or ConflictException or DbUpdateConcurrencyException
            => (StatusCodes.Status409Conflict, "Conflict"),
        DomainException
            => (StatusCodes.Status400BadRequest, "Validation failed"),
        NotFoundException
            => (StatusCodes.Status404NotFound, "Not found"),
        UnauthorizedAccessException
            => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        ForbiddenAccessException
            => (StatusCodes.Status403Forbidden, "Forbidden"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
    };

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception while processing {Method} {Path}.")]
    private static partial void LogUnhandledException(ILogger logger, string method, string path, Exception exception);
}
