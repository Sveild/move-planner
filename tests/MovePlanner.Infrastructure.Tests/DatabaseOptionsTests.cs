using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MovePlanner.Infrastructure.Tests;

public sealed class DatabaseOptionsTests
{
    private const string ConnectionStringKey = "Database:ConnectionString";

    [Fact]
    public void A_missing_connection_string_is_invalid()
    {
        using ServiceProvider provider = BuildProvider([]);

        Action resolve = () => ResolveOptions(provider);

        resolve.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void An_empty_connection_string_is_invalid()
    {
        using ServiceProvider provider = BuildProvider(
            [new KeyValuePair<string, string?>(ConnectionStringKey, string.Empty)]);

        Action resolve = () => ResolveOptions(provider);

        resolve.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void A_connection_string_is_bound_from_configuration()
    {
        using ServiceProvider provider = BuildProvider(
            [new KeyValuePair<string, string?>(ConnectionStringKey, "Host=localhost;Database=moveplanner")]);

        DatabaseOptions options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        options.ConnectionString.Should().Be("Host=localhost;Database=moveplanner");
    }

    private static ServiceProvider BuildProvider(KeyValuePair<string, string?>[] settings)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    private static void ResolveOptions(ServiceProvider provider)
    {
        _ = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    }
}
