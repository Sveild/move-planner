using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using TestResult = NetArchTest.Rules.TestResult;

namespace MovePlanner.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string DomainAssembly = "MovePlanner.Domain";
    private const string ApplicationAssembly = "MovePlanner.Application";
    private const string InfrastructureAssembly = "MovePlanner.Infrastructure";
    private const string ApiAssembly = "MovePlanner.Api";

    [Fact]
    public void Domain_depends_on_no_other_layer()
    {
        AssertNoDependencyOn(DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly);
    }

    [Fact]
    public void Domain_depends_on_no_aspnetcore_or_efcore_types()
    {
        AssertNoDependencyOn(DomainAssembly, "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Application_depends_on_domain_only()
    {
        AssertNoDependencyOn(
            ApplicationAssembly,
            InfrastructureAssembly,
            ApiAssembly,
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Infrastructure_depends_on_no_api()
    {
        AssertNoDependencyOn(InfrastructureAssembly, ApiAssembly);
    }

    [Fact]
    public void Handlers_depend_on_no_http_types()
    {
        TestResult result = Types
            .InAssembly(Assembly.Load(ApplicationAssembly))
            .That()
            .HaveNameEndingWith("Handler", StringComparison.Ordinal)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "handlers must not reference HTTP types, but these do: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Result_and_error_types_no_longer_exist()
    {
        string[] removed = ["Result", "Result`1", "Error", "ErrorKind", "ResultExtensions"];

        string[] remaining =
        [
            .. new[] { ApplicationAssembly, ApiAssembly }
                .SelectMany(assembly => Assembly.Load(assembly).GetTypes())
                .Select(type => type.Name)
                .Where(name => removed.Contains(name, StringComparer.Ordinal)),
        ];

        remaining.Should().BeEmpty(
            "handlers signal failure by throwing, not by returning Result or Error values (ADR-0003)");
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
}
