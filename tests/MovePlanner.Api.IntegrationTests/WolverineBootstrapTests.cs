using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovePlanner.Application.Diagnostics;
using MovePlanner.Infrastructure;
using Wolverine;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class WolverineBootstrapTests : IClassFixture<PostgresApiFactory>, IDisposable
{
    private readonly PostgresApiFactory _factory;
    private readonly IServiceScope _scope;
    private readonly IMessageBus _bus;

    public WolverineBootstrapTests(PostgresApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory;
        _scope = factory.Services.CreateScope();
        _bus = _scope.ServiceProvider.GetRequiredService<IMessageBus>();
    }

    public void Dispose()
    {
        _scope.Dispose();
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
    public async Task Wolverine_provisions_its_message_storage_in_postgresql()
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        List<string> tables = await context.Database
            .SqlQuery<string>(
                $"select table_name as \"Value\" from information_schema.tables where table_schema = {WolverinePersistenceExtensions.MessageStoreSchemaName}")
            .ToListAsync(TestContext.Current.CancellationToken);

        tables.Should().Contain("wolverine_incoming_envelopes");
        tables.Should().Contain("wolverine_outgoing_envelopes");
    }

    [Fact]
    public async Task The_application_context_maps_wolverines_envelope_storage()
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        string[] mapped = [.. context.Model.GetEntityTypes()
            .Where(entity => string.Equals(
                entity.GetSchema(),
                WolverinePersistenceExtensions.MessageStoreSchemaName,
                StringComparison.Ordinal))
            .Select(entity => entity.GetTableName() ?? string.Empty)];

        mapped.Should().BeEquivalentTo("wolverine_incoming_envelopes", "wolverine_outgoing_envelopes");
    }

    [Fact]
    public void UseMovePlannerWolverine_rejects_a_null_builder()
    {
        IHostBuilder? builder = null;

        Action act = () => builder!.UseMovePlannerWolverine(
            new ConfigurationBuilder().Build(),
            Host.CreateApplicationBuilder([]).Environment);

        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public void UseMovePlannerWolverine_rejects_a_null_configuration()
    {
        IHostBuilder builder = new HostBuilder();

        Action act = () => builder.UseMovePlannerWolverine(
            null!,
            Host.CreateApplicationBuilder([]).Environment);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void UseMovePlannerWolverine_rejects_a_null_environment()
    {
        IHostBuilder builder = new HostBuilder();

        Action act = () => builder.UseMovePlannerWolverine(
            new ConfigurationBuilder().Build(),
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("environment");
    }
}
