using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MovePlanner.Application.Common;

namespace MovePlanner.Application;

/// <summary>Registers the services of the Application layer.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every command and query handler found in the Application assembly as a scoped
    /// service, so a new handler is picked up without any manual registration.
    /// </summary>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <returns>The same <paramref name="services"/> instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHandlers(typeof(ServiceCollectionExtensions).Assembly);
    }

    /// <summary>
    /// Registers every command and query handler found in <paramref name="assembly"/> as a scoped
    /// service. Internal so the scan can be tested against an assembly that contains handlers.
    /// </summary>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The same <paramref name="services"/> instance, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="assembly"/> is <see langword="null"/>.
    /// </exception>
    internal static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            {
                continue;
            }

            foreach (Type serviceType in type.GetInterfaces().Where(IsHandlerInterface))
            {
                services.AddScoped(serviceType, type);
            }
        }

        return services;
    }

    private static bool IsHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        Type definition = type.GetGenericTypeDefinition();

        return definition.Equals(typeof(ICommandHandler<,>)) || definition.Equals(typeof(IQueryHandler<,>));
    }
}
