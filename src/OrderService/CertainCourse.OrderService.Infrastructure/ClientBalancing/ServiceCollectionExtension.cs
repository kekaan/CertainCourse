using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.Application.ClientBalancing;

namespace CertainCourse.OrderService.Infrastructure.ClientBalancing;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddClientBalancing(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IServiceDiscoveryClient, ServiceDiscoveryClient>();
        serviceCollection.AddSingleton<IDbStore, DbStore>();

        return serviceCollection;
    }
}