using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using TestResult = NetArchTest.Rules.TestResult;

namespace MovePlanner.Architecture.Tests;

public sealed class WolverineConventionTests
{
    [Fact]
    public void Handlers_live_in_the_application_assembly()
    {
        Type[] outside =
        [
            .. Architecture.TypesIn(Architecture.ExceptApplication).Where(Architecture.IsHandler),
        ];

        outside.Should().BeEmpty(
            "Wolverine discovers handlers in the Application assembly, but these types elsewhere have a Handle method: {0}",
            Architecture.Named(outside));
    }

    [Fact]
    public void Handlers_reference_no_http_types()
    {
        TestResult result = Types
            .InAssembly(Architecture.Application)
            .That()
            .HaveNameEndingWith("Handler", StringComparison.Ordinal)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.AspNetCore", "Wolverine.Http")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "handlers must not reference HTTP types such as IResult, but these do: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));

        Type[] handlers = [.. Architecture.TypesIn([Architecture.Application]).Where(Architecture.IsHandler)];

        Type[] httpSignatures =
        [
            .. handlers.Where(handler => Architecture.HandlerMethods(handler).Any(Architecture.UsesHttpTypes)),
        ];

        httpSignatures.Should().BeEmpty(
            "handler signatures stay transport-neutral, but these handlers take or return HTTP types: {0}",
            Architecture.Named(httpSignatures));
    }

    [Fact]
    public void Endpoint_classes_live_in_the_api_assembly()
    {
        Type[] outside =
        [
            .. Architecture.TypesIn(Architecture.ExceptApi).Where(Architecture.IsEndpoint),
        ];

        outside.Should().BeEmpty(
            "endpoints are part of the Api layer, but these types elsewhere carry Wolverine.HTTP attributes: {0}",
            Architecture.Named(outside));
    }

    [Fact]
    public void Commands_queries_and_dtos_are_immutable_records()
    {
        Type[] offenders =
        [
            .. Architecture.ContractTypes.Where(contract =>
                !Architecture.IsRecord(contract) || !Architecture.IsImmutable(contract)),
        ];

        offenders.Should().BeEmpty(
            "commands, queries and DTOs are immutable records, but these are not: {0}",
            Architecture.Named(offenders));
    }

    [Fact]
    public void Commands_queries_and_dtos_carry_no_http_binding_attributes()
    {
        Type[] offenders =
        [
            .. Architecture.ContractTypes.Where(Architecture.CarriesHttpBindingAttribute),
        ];

        offenders.Should().BeEmpty(
            "the Application contract is transport-neutral, but these types carry FromRoute, FromQuery or FromBody: {0}",
            Architecture.Named(offenders));
    }

    [Fact]
    public void Validators_live_in_the_application_assembly()
    {
        Type[] outside =
        [
            .. Architecture.TypesIn(Architecture.ExceptApplication)
                .Where(type => type.Name.EndsWith("Validator", StringComparison.Ordinal)),
        ];

        outside.Should().BeEmpty(
            "validators run in Wolverine's handler middleware next to the handler, but these live elsewhere: {0}",
            Architecture.Named(outside));
    }

    [Fact]
    public void Handlers_return_the_response_type_or_nothing()
    {
        Type[] handlers = [.. Architecture.TypesIn([Architecture.Application]).Where(Architecture.IsHandler)];

        Type[] wrappers =
        [
            .. handlers.SelectMany(Architecture.HandlerMethods)
                .Where(Architecture.ReturnsResultWrapper)
                .Select(method => method.DeclaringType!)
                .Distinct(),
        ];

        wrappers.Should().BeEmpty(
            "handlers return the response value or throw, so there is no Result wrapper: {0}",
            Architecture.Named(wrappers));
    }

    [Fact]
    public void Result_and_error_types_no_longer_exist()
    {
        string[] removed = ["Result", "Result`1", "Error", "ErrorKind", "ResultExtensions"];

        Type[] remaining =
        [
            .. Architecture.TypesIn([Architecture.Application, Architecture.Api])
                .Where(type => removed.Contains(type.Name, StringComparer.Ordinal)),
        ];

        remaining.Should().BeEmpty(
            "handlers signal failure by throwing, not by returning Result or Error values (ADR-0003)");
    }

    [Fact]
    public void Custom_handler_contract_and_registration_no_longer_exist()
    {
        string[] removed =
        [
            "ICommandHandler`2", "IQueryHandler`2", "Unit",
            "HandlerEndpointExtensions", "IQueryRequest`1", "ICommandRequest`1",
            "EndpointsExtensions",
        ];

        Type[] remaining =
        [
            .. Architecture.TypesIn([Architecture.Application, Architecture.Api])
                .Where(type => removed.Contains(type.Name, StringComparer.Ordinal)),
        ];

        remaining.Should().BeEmpty(
            "handlers are found by Wolverine's discovery and called through IMessageBus, "
            + "not through a custom contract or hand-written endpoint mapping (ADR-0002, ADR-0005)");
    }

    [Fact]
    public void Expected_failures_are_exceptions_and_the_api_defines_none()
    {
        Type[] apiExceptions =
        [
            .. Architecture.TypesIn([Architecture.Api])
                .Where(type => typeof(Exception).IsAssignableFrom(type)),
        ];

        apiExceptions.Should().BeEmpty(
            "the Api maps exceptions to responses instead of defining its own, but these exist: {0}",
            Architecture.Named(apiExceptions));

        Type[] failureTypes =
        [
            .. Architecture.TypesIn([Architecture.Domain, Architecture.Application])
                .Where(type => type.Name.EndsWith("Exception", StringComparison.Ordinal)),
        ];

        failureTypes.Should().NotBeEmpty("expected failures are expressed as exceptions (ADR-0003)");
        failureTypes.Should().OnlyContain(type => typeof(Exception).IsAssignableFrom(type));
    }
}
