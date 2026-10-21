using FluentAssertions;
using Microsoft.OpenApi;
using MovePlanner.Api.OpenApi;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

/// <summary>
/// Covers the two document conventions the API applies: camelCase query string parameters and the
/// removal of the placeholder response OpenAPI infers for endpoints that return <c>IResult</c>.
/// </summary>
public sealed class OpenApiTransformerTests
{
    [Fact]
    public async Task Query_string_parameters_are_documented_in_camel_case()
    {
        OpenApiOperation operation = new()
        {
            Parameters =
            [
                new OpenApiParameter { Name = "Message", In = ParameterLocation.Query },
                new OpenApiParameter { Name = "X-Trace-Id", In = ParameterLocation.Header },
            ],
        };

        await new CamelCaseQueryParameterTransformer().TransformAsync(
            operation,
            null!,
            CancellationToken.None);

        operation.Parameters.Select(parameter => parameter.Name)
            .Should().BeEquivalentTo("message", "X-Trace-Id");
    }

    [Fact]
    public async Task An_unnamed_query_string_parameter_is_left_alone()
    {
        OpenApiOperation operation = new()
        {
            Parameters =
            [
                new OpenApiParameter { Name = string.Empty, In = ParameterLocation.Query },
            ],
        };

        await new CamelCaseQueryParameterTransformer().TransformAsync(
            operation,
            null!,
            CancellationToken.None);

        operation.Parameters.Single().Name.Should().BeEmpty();
    }

    [Fact]
    public async Task A_query_string_parameter_without_a_name_is_left_alone()
    {
        OpenApiOperation operation = new()
        {
            Parameters =
            [
                new OpenApiParameter { Name = null!, In = ParameterLocation.Query },
            ],
        };

        await new CamelCaseQueryParameterTransformer().TransformAsync(
            operation,
            null!,
            CancellationToken.None);

        operation.Parameters.Single().Name.Should().BeNull();
    }
    [Fact]
    public async Task An_operation_without_parameters_is_left_alone()
    {
        OpenApiOperation operation = new() { Parameters = null! };

        await new CamelCaseQueryParameterTransformer().TransformAsync(
            operation,
            null!,
            CancellationToken.None);

        operation.Parameters.Should().BeNull();
    }

    [Fact]
    public async Task A_declared_success_response_replaces_the_inferred_result_response()
    {
        OpenApiOperation operation = new()
        {
            Responses = new OpenApiResponses
            {
                ["200"] = CreateResponse("IResult"),
                ["201"] = CreateResponse("PingQuery"),
            },
        };

        await new ResultResponseTransformer().TransformAsync(operation, null!, CancellationToken.None);

        operation.Responses.Keys.Should().BeEquivalentTo("201");
    }

    [Fact]
    public async Task The_inferred_result_response_stays_when_nothing_else_describes_success()
    {
        OpenApiOperation operation = new()
        {
            Responses = new OpenApiResponses
            {
                ["200"] = CreateResponse("IResult"),
                ["500"] = CreateResponse("ProblemDetails"),
            },
        };

        await new ResultResponseTransformer().TransformAsync(operation, null!, CancellationToken.None);

        operation.Responses.Keys.Should().BeEquivalentTo("200", "500");
    }

    [Fact]
    public async Task A_response_without_a_body_is_not_mistaken_for_the_inferred_result_response()
    {
        OpenApiOperation operation = new()
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse(),
                ["201"] = CreateResponse("PingQuery"),
                ["204"] = new OpenApiResponse(),
            },
        };

        await new ResultResponseTransformer().TransformAsync(operation, null!, CancellationToken.None);

        operation.Responses.Keys.Should().BeEquivalentTo("200", "201", "204");
    }

    [Fact]
    public async Task An_operation_without_responses_is_left_alone()
    {
        OpenApiOperation operation = new() { Responses = null! };

        await new ResultResponseTransformer().TransformAsync(operation, null!, CancellationToken.None);

        operation.Responses.Should().BeNull();
    }

    private static OpenApiResponse CreateResponse(string schemaId) => new()
    {
        Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
        {
            ["application/json"] = new()
            {
                Schema = new OpenApiSchemaReference(schemaId, new OpenApiDocument(), null),
            },
        },
    };
}
