using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MovePlanner.Infrastructure.Tests;

public sealed class AppDbContextTests
{
    private const string ConnectionString = "Host=localhost;Database=moveplanner";

    [Fact]
    public void The_database_context_is_registered_with_the_npgsql_provider()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.ProviderName.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");
        context.Database.GetConnectionString().Should().Be(ConnectionString);
    }

    [Fact]
    public void The_design_time_factory_creates_a_context_from_the_environment()
    {
        var factory = new AppDbContextFactory();
        Environment.SetEnvironmentVariable(AppDbContextFactory.ConnectionStringVariable, ConnectionString);

        try
        {
            using AppDbContext context = factory.CreateDbContext([]);

            context.Database.ProviderName.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");
            context.Database.GetConnectionString().Should().Be(ConnectionString);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AppDbContextFactory.ConnectionStringVariable, null);
        }
    }

    [Fact]
    public void The_design_time_factory_requires_a_connection_string()
    {
        var factory = new AppDbContextFactory();
        Environment.SetEnvironmentVariable(AppDbContextFactory.ConnectionStringVariable, null);

        Action create = () =>
        {
            using AppDbContext context = factory.CreateDbContext([]);
        };

        create.Should().Throw<InvalidOperationException>();
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                [new KeyValuePair<string, string?>("Database:ConnectionString", ConnectionString)])
            .Build();
    }
}
