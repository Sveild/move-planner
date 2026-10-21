using Microsoft.AspNetCore.Mvc;
using MovePlanner.Application.Diagnostics;
using Wolverine;
using Wolverine.Http;

namespace MovePlanner.Api.Endpoints.Diagnostics;

/// <summary>HTTP endpoints of the diagnostics feature.</summary>
public static class PingEndpoints
{
    /// <summary>
    /// Echoes <paramref name="query"/> back. The endpoint is a thin adapter: the Application query is
    /// bound straight from the query string, and its handler runs through the message bus, so the
    /// validation and transaction middleware that Wolverine applies to the handler apply here too.
    /// </summary>
    /// <param name="query">The query, bound from the query string by member name.</param>
    /// <param name="bus">The message bus that dispatches the query to its handler.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <returns>The message the query carries.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/>.</exception>
    [WolverineGet("/ping")]
    public static Task<string> Ping(
        [FromQuery] PingQuery query,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(bus);

        return bus.InvokeAsync<string>(query, cancellationToken);
    }
}
