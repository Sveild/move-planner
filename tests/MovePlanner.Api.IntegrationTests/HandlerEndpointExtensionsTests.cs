using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MovePlanner.Api.Endpoints;
using MovePlanner.Api.Errors;
using MovePlanner.Application.Common;
using MovePlanner.Application.Common.Exceptions;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

public sealed class HandlerEndpointExtensionsTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddScoped<IQueryHandler<EchoQuery, string>, EchoQueryHandler>();
        builder.Services.AddScoped<ICommandHandler<UppercaseCommand, string>, UppercaseCommandHandler>();
        builder.Services.AddScoped<ICommandHandler<RenameCommand, string>, RenameCommandHandler>();
        builder.Services.AddScoped<ICommandHandler<DeleteCommand, string>, DeleteCommandHandler>();

        _app = builder.Build();
        _app.UseExceptionHandler();

        RouteGroupBuilder dummy = _app.MapGroup("/dummy");
        dummy.MapGetHandler<EchoRequest, EchoQuery, string>("/{value}");
        dummy.MapPostHandler<UppercaseRequest, UppercaseCommand, string>("/uppercase");
        dummy.MapPutHandler<RenameRequest, RenameCommand, string>("/{id}");
        dummy.MapDeleteHandler<DeleteRequest, DeleteCommand, string>("/{value}");

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Get_handler_binds_the_request_maps_it_and_returns_the_response()
    {
        HttpResponseMessage response = await _client.GetAsync(Relative("/dummy/hello"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<string>(TestContext.Current.CancellationToken))
            .Should().Be("query:hello");
    }

    [Fact]
    public async Task Post_handler_binds_the_body_maps_it_and_returns_the_response()
    {
        using StringContent body = new("\"hello\"", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(Relative("/dummy/uppercase"), body, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<string>(TestContext.Current.CancellationToken))
            .Should().Be("HELLO");
    }

    [Fact]
    public async Task Put_handler_binds_the_route_and_the_body_maps_them_and_returns_the_response()
    {
        Guid id = Guid.NewGuid();
        using StringContent body = new("\"Hello\"", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PutAsync(Relative($"/dummy/{id}"), body, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<string>(TestContext.Current.CancellationToken))
            .Should().Be($"renamed:{id}:Hello");
    }

    [Fact]
    public async Task Delete_handler_binds_the_route_maps_it_and_returns_the_response()
    {
        HttpResponseMessage response = await _client.DeleteAsync(Relative("/dummy/hello"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<string>(TestContext.Current.CancellationToken))
            .Should().Be("deleted:hello");
    }

    [Fact]
    public async Task A_handler_exception_becomes_a_problem_response()
    {
        HttpResponseMessage response =
            await _client.GetAsync(Relative("/dummy/missing"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadFromJsonAsync<ProblemResponse>(TestContext.Current.CancellationToken))
            .Should().Be(new ProblemResponse(StatusCodes.Status404NotFound, "Not found"));
    }

    [Fact]
    public void Handlers_reject_a_null_route_builder()
    {
        IEndpointRouteBuilder? endpoints = null;

        Action[] map =
        [
            () => endpoints!.MapGetHandler<EchoRequest, EchoQuery, string>("/{value}"),
            () => endpoints!.MapPostHandler<UppercaseRequest, UppercaseCommand, string>("/uppercase"),
            () => endpoints!.MapPutHandler<RenameRequest, RenameCommand, string>("/{id}"),
            () => endpoints!.MapDeleteHandler<DeleteRequest, DeleteCommand, string>("/{value}"),
        ];

        map.Should().AllSatisfy(handler => handler.Should().Throw<ArgumentNullException>());
    }

    [Fact]
    public void Handlers_reject_a_blank_route_pattern()
    {
        Action[] map =
        [
            () => _app.MapGetHandler<EchoRequest, EchoQuery, string>(" "),
            () => _app.MapPostHandler<UppercaseRequest, UppercaseCommand, string>(" "),
            () => _app.MapPutHandler<RenameRequest, RenameCommand, string>(" "),
            () => _app.MapDeleteHandler<DeleteRequest, DeleteCommand, string>(" "),
        ];

        map.Should().AllSatisfy(handler => handler.Should().Throw<ArgumentException>());
    }

    private static Uri Relative(string path) => new(path, UriKind.Relative);

    private sealed record ProblemResponse(int Status, string Title);

    private sealed record EchoQuery(string Value);

    private sealed record UppercaseCommand(string Value);

    private sealed record RenameCommand(Guid Id, string Value);

    private sealed record DeleteCommand(string Value);

    private sealed class EchoRequest : IQueryRequest<EchoQuery>
    {
        [FromRoute]
        public string Value { get; set; } = string.Empty;

        public EchoQuery ToQuery() => new(Value);
    }

    private sealed class UppercaseRequest : ICommandRequest<UppercaseCommand>
    {
        [FromBody]
        public string Value { get; set; } = string.Empty;

        public UppercaseCommand ToCommand() => new(Value);
    }

    private sealed class RenameRequest : ICommandRequest<RenameCommand>
    {
        [FromRoute]
        public Guid Id { get; set; }

        [FromBody]
        public string Value { get; set; } = string.Empty;

        public RenameCommand ToCommand() => new(Id, Value);
    }

    private sealed class DeleteRequest : ICommandRequest<DeleteCommand>
    {
        [FromRoute]
        public string Value { get; set; } = string.Empty;

        public DeleteCommand ToCommand() => new(Value);
    }

    private sealed class EchoQueryHandler : IQueryHandler<EchoQuery, string>
    {
        public Task<string> HandleAsync(EchoQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(query.Value, "missing", StringComparison.Ordinal))
            {
                throw new NotFoundException("The dummy is missing.");
            }

            return Task.FromResult($"query:{query.Value}");
        }
    }

    private sealed class UppercaseCommandHandler : ICommandHandler<UppercaseCommand, string>
    {
        public Task<string> HandleAsync(UppercaseCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(command.Value.ToUpperInvariant());
        }
    }

    private sealed class RenameCommandHandler : ICommandHandler<RenameCommand, string>
    {
        public Task<string> HandleAsync(RenameCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult($"renamed:{command.Id}:{command.Value}");
        }
    }

    private sealed class DeleteCommandHandler : ICommandHandler<DeleteCommand, string>
    {
        public Task<string> HandleAsync(DeleteCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult($"deleted:{command.Value}");
        }
    }
}
