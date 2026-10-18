using JasperFx;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace MovePlanner.Infrastructure;

/// <summary>
/// Configures Wolverine's durable message storage and transactional middleware for this application.
/// </summary>
public static class WolverinePersistenceExtensions
{
    /// <summary>
    /// The PostgreSQL schema that holds Wolverine's message storage tables. Wolverine owns these tables
    /// (it creates and migrates them), and the Entity Framework Core mappings for them are excluded from
    /// the migrations, so this schema is not part of the Entity Framework Core migration pipeline.
    /// </summary>
    public const string MessageStoreSchemaName = "wolverine";

    /// <summary>
    /// Stores Wolverine's inbox and outbox in <paramref name="connectionString"/>, wraps handlers that use
    /// the database in a transaction that Entity Framework Core commits (and whose outbox it flushes) when
    /// the handler completes, and makes locally routed messages durable.
    /// </summary>
    /// <param name="options">The Wolverine options to configure.</param>
    /// <param name="connectionString">The PostgreSQL connection string of the application database.</param>
    /// <param name="isDevelopment">
    /// <see langword="true"/> in the Development environment, where Wolverine may create or update its
    /// message storage tables at startup. Outside Development the tables have to exist already, so a
    /// production deployment provisions them explicitly and never patches its schema implicitly.
    /// </param>
    /// <returns>The same <paramref name="options"/> instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="connectionString"/> is <see langword="null"/>, empty or whitespace.
    /// </exception>
    public static WolverineOptions PersistWithPostgresql(
        this WolverineOptions options,
        string? connectionString,
        bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        options.PersistMessagesWithPostgresql(connectionString, MessageStoreSchemaName);
        options.AutoBuildMessageStorageOnStartup = isDevelopment
            ? AutoCreate.CreateOrUpdate
            : AutoCreate.None;

        options.UseEntityFrameworkCoreTransactions();
        options.Policies.AutoApplyTransactions();
        options.Policies.UseDurableLocalQueues();

        return options;
    }
}
