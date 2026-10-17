namespace MovePlanner.Application.Common;

/// <summary>
/// A value that carries no information. It is the response type of a command that produces no
/// result, so such a command can still return <see cref="Result{T}"/> like every other handler.
/// </summary>
public readonly record struct Unit;
