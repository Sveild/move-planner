using MovePlanner.Tests.Support;
using Xunit;

namespace MovePlanner.Infrastructure.Tests;

[CollectionDefinition(Name)]
public sealed class PostgresCollectionDefinition : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Postgres";
}
