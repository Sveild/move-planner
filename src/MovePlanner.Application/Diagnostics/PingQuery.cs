namespace MovePlanner.Application.Diagnostics;

/// <summary>
/// A liveness query that echoes its message back. It touches nothing, which makes it a cheap way
/// to check that Wolverine's handler discovery reaches the Application assembly.
/// </summary>
/// <param name="Message">The message to echo back.</param>
public sealed record PingQuery(string Message);
