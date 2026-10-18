using Wolverine.Attributes;

namespace MovePlanner.Application.Diagnostics;

/// <summary>
/// Answers a <see cref="PingQuery"/> by echoing its message back. The class is static, so Wolverine
/// needs <see cref="WolverineHandlerAttribute"/> to discover it.
/// </summary>
[WolverineHandler]
public static class PingQueryHandler
{
    /// <summary>Handles <paramref name="query"/>.</summary>
    /// <param name="query">The query to handle.</param>
    /// <returns>The message carried by <paramref name="query"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null"/>.</exception>
    public static string Handle(PingQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Message;
    }
}
