using MovePlanner.Tests.Support;
using Xunit;

namespace MovePlanner.Api.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgresCollectionDefinition : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Postgres";
}
