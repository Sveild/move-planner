namespace MovePlanner.Api.Endpoints;

/// <summary>
/// The shape of an Api request that maps to an Application query. The properties of a request type
/// carry the binding attributes, for example <c>[FromRoute]</c> and <c>[FromQuery]</c>, which keeps
/// HTTP binding out of the Application layer.
/// </summary>
/// <typeparam name="TQuery">The query the request maps to.</typeparam>
public interface IQueryRequest<out TQuery>
{
    /// <summary>Creates the query that this request represents.</summary>
    /// <returns>The query to hand to the handler.</returns>
    TQuery ToQuery();
}
