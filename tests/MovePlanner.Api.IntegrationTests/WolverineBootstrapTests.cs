using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovePlanner.Application.Diagnostics;
using MovePlanner.Infrastructure;
using MovePlanner.Tests.Support;
using Wolverine;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class WolverineBootstrapTests : IDisposable
{
    private readonly PostgresApiFactory _factory;
    private readonly IServiceScope _scope;
    private readonly IMessageBus _bus;

    public WolverineBootstrapTests(PostgresContainerFixture postgres)
    {
        ArgumentNullException.ThrowIfNull(postgres);

        _factory = new PostgresApiFactory(postgres.ConnectionString);
        _scope = _factory.Services.CreateScope();
        _bus = _scope.ServiceProvider.GetRequiredService<IMessageBus>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task The_sample_handler_in_the_application_assembly_is_discovered()
    {
        string result = await _bus.InvokeAsync<string>(
            new PingQuery("pong"),
            TestContext.Current.CancellationToken);

        result.Should().Be("pong");
    }

    [Fact]
    public void UseMovePlannerWolverine_rejects_a_null_builder()
    {
        IHostBuilder? builder = null;

        Action act = () => builder!.UseMovePlannerWolverine();

        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    private sealed class PostgresApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public PostgresApiFactory(string connectionString) => _connectionString = connectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    [$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"] = _connectionString,
                }));
        }
    }
}
