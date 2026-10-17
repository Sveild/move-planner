using Microsoft.EntityFrameworkCore;

namespace MovePlanner.Infrastructure;

/// <summary>The Entity Framework Core database context of the application.</summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>Initializes a new instance of the <see cref="AppDbContext"/> class.</summary>
    /// <param name="options">The options used to configure the context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
