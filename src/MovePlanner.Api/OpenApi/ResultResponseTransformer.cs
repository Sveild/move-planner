using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MovePlanner.Api.OpenApi;

/// <summary>
/// Removes the response OpenAPI infers from an endpoint that returns <see cref="IResult"/>. The
/// generator cannot see which status code the endpoint picks, so it documents a 200 with an
/// <c>IResult</c> schema; an endpoint that declares its real success responses (201 on create, 204 on
/// delete) says what it returns, and that placeholder only misleads readers.
/// </summary>
public sealed class ResultResponseTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.Responses is null)
        {
            return Task.CompletedTask;
        }

        string[] inferred = [.. operation.Responses
            .Where(response => IsResultOnly(response.Value))
            .Select(response => response.Key)];

        bool declaresOtherSuccess = operation.Responses.Keys
            .Any(key => key.StartsWith('2') && !inferred.Contains(key, StringComparer.Ordinal));

        if (!declaresOtherSuccess)
        {
            return Task.CompletedTask;
        }

        foreach (string status in inferred)
        {
            operation.Responses.Remove(status);
        }

        return Task.CompletedTask;
    }

    private static bool IsResultOnly(IOpenApiResponse response) =>
        response is OpenApiResponse { Content.Count: > 0 } concrete
        && concrete.Content.Values.All(IsResultSchema);

    private static bool IsResultSchema(OpenApiMediaType mediaType) =>
        mediaType.Schema is OpenApiSchemaReference { Reference.Id: "IResult" };
}
