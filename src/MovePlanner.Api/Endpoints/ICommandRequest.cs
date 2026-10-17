namespace MovePlanner.Api.Endpoints;

/// <summary>
/// The shape of an Api request that maps to an Application command. The properties of a request type
/// carry the binding attributes, for example <c>[FromRoute]</c> and <c>[FromBody]</c>, which keeps
/// HTTP binding out of the Application layer.
/// </summary>
/// <typeparam name="TCommand">The command the request maps to.</typeparam>
public interface ICommandRequest<out TCommand>
{
    /// <summary>Creates the command that this request represents.</summary>
    /// <returns>The command to hand to the handler.</returns>
    TCommand ToCommand();
}
