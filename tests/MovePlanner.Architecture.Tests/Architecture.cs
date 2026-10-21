using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace MovePlanner.Architecture.Tests;

/// <summary>The assemblies under test and the reflection queries the rules are built on.</summary>
internal static class Architecture
{
    public const string DomainName = "MovePlanner.Domain";
    public const string ApplicationName = "MovePlanner.Application";
    public const string InfrastructureName = "MovePlanner.Infrastructure";
    public const string ApiName = "MovePlanner.Api";

    private static readonly Type[] _endpointAttributes =
    [
        typeof(WolverineGetAttribute),
        typeof(WolverinePostAttribute),
        typeof(WolverinePutAttribute),
        typeof(WolverineDeleteAttribute),
    ];

    private static readonly Type[] _bindingAttributes =
    [
        typeof(FromRouteAttribute),
        typeof(FromQueryAttribute),
        typeof(FromBodyAttribute),
    ];

    private static readonly string[] _contractSuffixes = ["Command", "Query", "Dto"];

    private const BindingFlags DeclaredMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
        | BindingFlags.DeclaredOnly;

    public static Assembly Domain => Assembly.Load(DomainName);

    public static Assembly Application => Assembly.Load(ApplicationName);

    public static Assembly Infrastructure => Assembly.Load(InfrastructureName);

    public static Assembly Api => Assembly.Load(ApiName);

    public static IReadOnlyList<Assembly> ExceptApi => [Domain, Application, Infrastructure];

    public static IReadOnlyList<Assembly> ExceptApplication => [Domain, Infrastructure, Api];

    public static bool References(Assembly assembly, string referencedAssembly) =>
        assembly.GetReferencedAssemblies()
            .Any(reference => string.Equals(reference.Name, referencedAssembly, StringComparison.Ordinal));

    public static string[] MatchingReferences(Assembly assembly, params string[] prefixes) =>
    [
        .. assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => prefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal))),
    ];

    public static bool UsesHttpTypes(MethodInfo method) =>
        new[] { method.ReturnType }
            .Concat(method.GetParameters().Select(parameter => parameter.ParameterType))
            .Any(IsHttpType);

    public static IEnumerable<Type> TypesIn(IEnumerable<Assembly> assemblies) =>
        assemblies.SelectMany(assembly => assembly.GetTypes());

    public static IEnumerable<Type> ContractTypes =>
        TypesIn([Application]).Where(type =>
            _contractSuffixes.Any(suffix => type.Name.EndsWith(suffix, StringComparison.Ordinal)));

    public static IEnumerable<MethodInfo> HandlerMethods(Type type) =>
        type.GetMethods(DeclaredMembers).Where(method => method.Name is "Handle" or "HandleAsync");

    public static bool IsHandler(Type type) => HandlerMethods(type).Any();

    public static bool IsEndpoint(Type type) => type.GetMethods(DeclaredMembers).Any(method =>
        method.GetCustomAttributes(inherit: false).Any(attribute =>
            _endpointAttributes.Contains(attribute.GetType())));

    public static bool IsRecord(Type type) =>
        type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance) is not null;

    public static bool IsImmutable(Type type) => type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .All(property => property.SetMethod is null
            || property.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)));

    public static bool CarriesHttpBindingAttribute(Type type) =>
        HasBindingAttribute(type.GetCustomAttributes(inherit: false))
        || type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(property => HasBindingAttribute(property.GetCustomAttributes(inherit: false)))
        || type.GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => HasBindingAttribute(parameter.GetCustomAttributes(inherit: false)));

    public static bool ReturnsResultWrapper(MethodInfo method)
    {
        Type type = method.ReturnType;

        return ContainsResultWrapper(type);
    }

    public static string Named(Type type) => $"{type.Assembly.GetName().Name}:{type.FullName}";

    public static string Named(IEnumerable<Type> types) => string.Join(", ", types.Select(Named));

    private static bool IsHttpType(Type type) =>
        type.Namespace?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("Wolverine.Http", StringComparison.Ordinal) == true
        || (type.IsGenericType && type.GetGenericArguments().Any(IsHttpType));

    private static bool ContainsResultWrapper(Type type) =>
        type.Name.StartsWith("Result", StringComparison.Ordinal)
        || (type.IsGenericType && type.GetGenericArguments().Any(ContainsResultWrapper));

    private static bool HasBindingAttribute(IEnumerable<object> attributes) =>
        attributes.Any(attribute => _bindingAttributes.Contains(attribute.GetType()));
}
