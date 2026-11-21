using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Laundry.BuildingBlocks.Api;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in types)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IEndpoint), type));
        }

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routes)
    {
        foreach (var endpoint in routes.ServiceProvider.GetServices<IEndpoint>())
        {
            endpoint.Map(routes);
        }

        return routes;
    }
}
