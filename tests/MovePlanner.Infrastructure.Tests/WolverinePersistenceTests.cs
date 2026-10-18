using FluentAssertions;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Xunit;

namespace MovePlanner.Infrastructure.Tests;

public sealed class WolverinePersistenceTests
{
    private const string ConnectionString = "Host=localhost;Database=moveplanner";

    [Fact]
    public void Persisting_with_postgresql_requires_options()
    {
        Action configure = () =>
            WolverinePersistenceExtensions.PersistWithPostgresql(null!, ConnectionString, false);

        configure.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Persisting_with_postgresql_requires_a_connection_string()
    {
        Action configure = () => new WolverineOptions().PersistWithPostgresql(" ", false);

        configure.Should().Throw<ArgumentException>().WithParameterName("connectionString");
    }

    [Fact]
    public async Task Development_may_create_or_update_the_message_storage()
    {
        await using var options = new WolverineOptions();

        options.PersistWithPostgresql(ConnectionString, true);

        options.AutoBuildMessageStorageOnStartup.Should().Be(AutoCreate.CreateOrUpdate);
    }

    [Fact]
    public async Task Any_other_environment_leaves_the_message_storage_alone()
    {
        await using var options = new WolverineOptions();

        options.PersistWithPostgresql(ConnectionString, false);

        options.AutoBuildMessageStorageOnStartup.Should().Be(AutoCreate.None);
    }

    [Fact]
    public void The_migrations_do_not_own_wolverines_message_storage()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        using var context = new AppDbContext(optionsBuilder.Options);

        context.Model.GetEntityTypes()
            .Select(entity => entity.Name)
            .Should().NotContain(name => name.StartsWith("Wolverine.", StringComparison.Ordinal));
    }
}
