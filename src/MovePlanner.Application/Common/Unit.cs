namespace MovePlanner.Application.Common;

/// <summary>
/// A value that carries no information. It is the response type of a command that produces no
/// result, so such a command can still implement <see cref="ICommandHandler{TCommand, TResponse}"/>
/// like every other one.
/// </summary>
public readonly record struct Unit;
