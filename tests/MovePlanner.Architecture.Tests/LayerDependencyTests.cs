using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace MovePlanner.Architecture.Tests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_does_not_depend_on_other_layers()
    {
        NetArchTest.Rules.TestResult result = Types
            .InAssembly(Assembly.Load("MovePlanner.Domain"))
            .Should()
            .NotHaveDependencyOnAny("MovePlanner.Application", "MovePlanner.Infrastructure", "MovePlanner.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must not depend on the outer layers, but these types do: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }
}
