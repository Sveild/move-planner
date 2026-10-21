using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MovePlanner.Api.OpenApi;

/// <summary>
/// Documents query string parameters in camelCase. Wolverine flattens a <c>[FromQuery]</c> record into
/// one parameter per member and names each parameter after the member, so <c>PingQuery.Message</c>
/// would be documented as <c>Message</c>. Binding ignores case, so the lowercase name is the form
/// clients should send and the one the JSON bodies already use.
/// </summary>
public sealed class CamelCaseQueryParameterTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.Parameters is null)
        {
            return Task.CompletedTask;
        }

        for (int index = 0; index < operation.Parameters.Count; index++)
        {
            IOpenApiParameter parameter = operation.Parameters[index];

            if (parameter.In == ParameterLocation.Query && parameter.Name is { Length: > 0 } name)
            {
                ((OpenApiParameter)parameter).Name = JsonNamingPolicy.CamelCase.ConvertName(name);
            }
        }

        return Task.CompletedTask;
    }
}
