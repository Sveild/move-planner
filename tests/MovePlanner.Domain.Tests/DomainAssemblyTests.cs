using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MovePlanner.Domain.Tests;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void Domain_assembly_is_referenced_and_loadable()
    {
        Assembly domain = Assembly.Load("MovePlanner.Domain");

        domain.GetName().Name.Should().Be("MovePlanner.Domain");
    }
}
