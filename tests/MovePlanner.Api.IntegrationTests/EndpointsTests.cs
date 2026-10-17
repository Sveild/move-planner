using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MovePlanner.Api.Endpoints;
using MovePlanner.Api.Endpoints.JobApplications;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

public sealed class EndpointsTests
{
    [Fact]
    public void MapEndpoints_maps_the_feature_groups_and_returns_the_application()
    {
        using WebApplication app = WebApplication.CreateBuilder().Build();

        app.MapEndpoints().Should().BeSameAs(app);
    }

    [Fact]
    public void MapEndpoints_rejects_a_null_application()
    {
        WebApplication? app = null;

        Action map = () => app!.MapEndpoints();

        map.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MapJobApplications_maps_the_route_group_of_the_feature()
    {
        using WebApplication app = WebApplication.CreateBuilder().Build();

        IEndpointRouteBuilder routeBuilder = app;
        app.MapJobApplications().MapGet("/", () => "ok");

        string[] patterns = [.. routeBuilder.DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText ?? string.Empty)];

        patterns.Should().Equal("/job-applications/");
    }

    [Fact]
    public void MapJobApplications_rejects_a_null_route_builder()
    {
        IEndpointRouteBuilder? endpoints = null;

        Action map = () => endpoints!.MapJobApplications();

        map.Should().Throw<ArgumentNullException>();
    }
}
