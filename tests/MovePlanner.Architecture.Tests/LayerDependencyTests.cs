using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using TestResult = NetArchTest.Rules.TestResult;

namespace MovePlanner.Architecture.Tests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_depends_on_no_other_layer()
    {
        AssertNoDependencyOn(
            Architecture.DomainName,
            Architecture.ApplicationName,
            Architecture.InfrastructureName,
            Architecture.ApiName);

        AssertNoReferenceTo(Architecture.Domain, "MovePlanner");
    }

    [Fact]
    public void Domain_depends_on_no_wolverine_aspnetcore_or_efcore_types()
    {
        AssertNoDependencyOn(
            Architecture.DomainName,
            "Wolverine",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore");

        AssertNoReferenceTo(Architecture.Domain, "Wolverine", "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Application_depends_on_domain_only()
    {
        AssertNoDependencyOn(
            Architecture.ApplicationName,
            Architecture.InfrastructureName,
            Architecture.ApiName,
            "Wolverine.Http",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore");

        AssertNoReferenceTo(
            Architecture.Application,
            Architecture.InfrastructureName,
            Architecture.ApiName,
            "Wolverine.Http",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Application_may_reference_wolverine_but_not_its_http_package()
    {
        Architecture.References(Architecture.Application, "Wolverine").Should().BeTrue(
            "handlers are found through Wolverine's own types, so Application must reference WolverineFx");

        Architecture.MatchingReferences(Architecture.Application, "Wolverine.Http").Should().BeEmpty(
            "only the Api layer talks HTTP, but Application references: {0}",
            string.Join(", ", Architecture.MatchingReferences(Architecture.Application, "Wolverine.Http")));
    }

    [Fact]
    public void Only_the_api_references_wolverine_http()
    {
        foreach (Assembly layer in Architecture.ExceptApi)
        {
            string[] references = Architecture.MatchingReferences(layer, "Wolverine.Http");

            references.Should().BeEmpty(
                "{0} must not reference Wolverine.HTTP, but it references: {1}",
                layer.GetName().Name,
                string.Join(", ", references));
        }

        Architecture.MatchingReferences(Architecture.Api, "Wolverine.Http").Should().NotBeEmpty(
            "the endpoints are built on Wolverine.HTTP");
    }

    [Fact]
    public void Infrastructure_may_reference_wolverine_persistence_but_not_the_api()
    {
        Architecture.References(Architecture.Infrastructure, "Wolverine.EntityFrameworkCore").Should().BeTrue(
            "Wolverine's outbox is integrated into the DbContext registration, so Infrastructure may reference its persistence packages");

        AssertNoDependencyOn(Architecture.InfrastructureName, Architecture.ApiName);
        Architecture.References(Architecture.Infrastructure, Architecture.ApiName).Should().BeFalse();
    }

    private static void AssertNoDependencyOn(string assembly, params string[] forbidden)
    {
        TestResult result = Types
            .InAssembly(Assembly.Load(assembly))
            .Should()
            .NotHaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "{0} must not depend on {1}, but these types do: {2}",
            assembly,
            string.Join(", ", forbidden),
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    private static void AssertNoReferenceTo(Assembly assembly, params string[] forbidden)
    {
        string[] references = Architecture.MatchingReferences(assembly, forbidden);

        references.Should().BeEmpty(
            "{0} must not reference {1}, but it references: {2}",
            assembly.GetName().Name,
            string.Join(", ", forbidden),
            string.Join(", ", references));
    }
}
