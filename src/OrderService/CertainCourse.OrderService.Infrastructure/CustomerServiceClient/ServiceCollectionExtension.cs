using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.Application.CustomerServiceClient;

namespace CertainCourse.OrderService.Infrastructure.CustomerServiceClient;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCustomerServiceClient(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ICustomerServiceClient, CustomerServiceClient>();
    }
}