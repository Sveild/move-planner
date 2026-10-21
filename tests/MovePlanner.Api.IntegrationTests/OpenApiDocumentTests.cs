using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

/// <summary>
/// Covers the published API description: the document itself and the Scalar UI that renders it. The
/// application serves both in Development only.
/// </summary>
[Collection(PostgresCollectionDefinition.Name)]
public sealed class OpenApiDocumentTests : IClassFixture<PostgresApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public OpenApiDocumentTests(PostgresApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task The_ping_endpoint_is_documented_with_its_parameter_and_responses()
    {
        JsonNode operation = await GetOperationAsync("/ping", "get");

        operation["tags"]!.AsArray().Select(tag => tag!.GetValue<string>())
            .Should().ContainSingle().Which.Should().Be("Diagnostics");

        JsonArray parameters = operation["parameters"]!.AsArray();
        parameters.Should().ContainSingle();

        JsonNode parameter = parameters[0]!;
        parameter["name"]!.GetValue<string>().Should().Be("message");
        parameter["in"]!.GetValue<string>().Should().Be("query");
        parameter["schema"]!["type"]!.GetValue<string>().Should().Be("string");

        JsonNode responses = operation["responses"]!;
        responses["200"]!["content"]!["text/plain"]!["schema"]!["type"]!.GetValue<string>()
            .Should().Be("string");
        responses["500"]!["content"]!["application/problem+json"]!["schema"]!["$ref"]!
            .GetValue<string>().Should().Be("#/components/schemas/ProblemDetails");
    }

    [Fact]
    public async Task The_scalar_ui_is_served_in_development()
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/scalar/v1", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        html.Should().Contain("Scalar API Reference");
    }

    private async Task<JsonNode> GetOperationAsync(string path, string method)
    {
        using HttpResponseMessage response = await _client.GetAsync(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        JsonNode? document = JsonNode.Parse(json);
        document.Should().NotBeNull();

        JsonNode? operation = document["paths"]?[path]?[method];
        operation.Should().NotBeNull();

        return operation;
    }
}
