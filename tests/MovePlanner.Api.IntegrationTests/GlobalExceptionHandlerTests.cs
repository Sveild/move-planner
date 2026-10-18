using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovePlanner.Api.Errors;
using MovePlanner.Application.Common.Exceptions;
using MovePlanner.Domain.Exceptions;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

public sealed class GlobalExceptionHandlerTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        _app = builder.Build();
        _app.UseExceptionHandler();
        _app.Run(context => throw CreateException(context.Request.Path.Value));

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Theory]
    [InlineData("/domain", HttpStatusCode.BadRequest)]
    [InlineData("/transition", HttpStatusCode.Conflict)]
    [InlineData("/conflict", HttpStatusCode.Conflict)]
    [InlineData("/not-found", HttpStatusCode.NotFound)]
    [InlineData("/unauthorized", HttpStatusCode.Unauthorized)]
    [InlineData("/forbidden", HttpStatusCode.Forbidden)]
    [InlineData("/concurrency", HttpStatusCode.Conflict)]
    [InlineData("/unknown", HttpStatusCode.InternalServerError)]
    public async Task An_exception_becomes_the_status_of_its_kind(string path, HttpStatusCode expected)
    {
        HttpResponseMessage response = await _client.GetAsync(
            new Uri(path, UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task An_expected_failure_reports_its_message()
    {
        HttpResponseMessage response = await _client.GetAsync(
            new Uri("/not-found", UriKind.Relative),
            TestContext.Current.CancellationToken);

        ProblemResponse? problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(
            TestContext.Current.CancellationToken);

        problem.Should().Be(new ProblemResponse(StatusCodes.Status404NotFound, "Not found", "The thing is missing."));
    }

    [Fact]
    public async Task An_unexpected_failure_hides_its_message()
    {
        HttpResponseMessage response = await _client.GetAsync(
            new Uri("/unknown", UriKind.Relative),
            TestContext.Current.CancellationToken);

        ProblemResponse? problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(
            TestContext.Current.CancellationToken);

        problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problem.Title.Should().Be("Unexpected error");
        problem.Detail.Should().BeNullOrEmpty();
    }

    private static Exception CreateException(string? path) => path switch
    {
        "/domain" => new DomainException("A rule was violated."),
        "/transition" => new InvalidStatusTransitionException("Rejected."),
        "/conflict" => new ConflictException("The application already exists."),
        "/not-found" => new NotFoundException("The thing is missing."),
        "/unauthorized" => new UnauthorizedAccessException("Sign in first."),
        "/forbidden" => new ForbiddenAccessException("Another user owns the application."),
        "/concurrency" => new DbUpdateConcurrencyException("The row changed."),
        _ => new InvalidOperationException("Boom."),
    };

    private sealed record ProblemResponse(int Status, string Title, string? Detail);
}
