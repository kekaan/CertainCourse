using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.Application.LogisticSimulatorClient;

namespace CertainCourse.OrderService.Infrastructure.LogisticSimulatorClient;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddLogisticSimulatorClient(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ILogisticSimulatorClient, LogisticSimulatorClient>();
    }
}