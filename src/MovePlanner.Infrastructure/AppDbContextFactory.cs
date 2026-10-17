using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MovePlanner.Infrastructure;

/// <summary>
/// Creates an <see cref="AppDbContext"/> for the Entity Framework Core command-line tools, so
/// migrations can be added and listed without starting the application.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>The environment variable the tools read the database connection string from.</summary>
    public const string ConnectionStringVariable = "Database__ConnectionString";

    /// <summary>
    /// Creates an <see cref="AppDbContext"/> from the connection string in the
    /// <see cref="ConnectionStringVariable"/> environment variable.
    /// </summary>
    /// <param name="args">The arguments passed by the tools. Not used.</param>
    /// <returns>A new <see cref="AppDbContext"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ConnectionStringVariable"/> environment variable is not set.
    /// </exception>
    public AppDbContext CreateDbContext(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set the {ConnectionStringVariable} environment variable to the database connection string before running Entity Framework Core commands.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
