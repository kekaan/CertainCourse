using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Infrastructure.ClientBalancing;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.CustomerServiceClient;
using CertainCourse.OrderService.Infrastructure.DataAccess;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres;
using CertainCourse.OrderService.Infrastructure.Kafka;
using CertainCourse.OrderService.Infrastructure.LogisticSimulatorClient;
using CertainCourse.OrderService.Infrastructure.Redis;

namespace CertainCourse.OrderService.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection
            .AddDefinedOptions(configuration)
            .AddRedis()
            .AddClientBalancing()
            .AddLogisticSimulatorClient()
            .AddCustomerServiceClient()
            .AddShardedDatabase()
            .AddRepository()
            .AddGrpcClients()
            .AddKafka();
    }

    private static IServiceCollection AddGrpcClients(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddServiceDiscoveryGrpcClient()
            .AddLogisticsSimulatorGrpcClient()
            .AddCustomerServiceGrpcClient();
    }

    private static IServiceCollection AddServiceDiscoveryGrpcClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddGrpcClient<Sd.Grpc.SdService.SdServiceClient>(
            (serviceProvider, options) =>
            {
                var grpcClientsOptions = serviceProvider.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;

                options.Address = new Uri(grpcClientsOptions.ServiceDiscoveryClientAddress);
            });

        return serviceCollection;
    }

    private static IServiceCollection AddLogisticsSimulatorGrpcClient(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddGrpcClient<LogisticsSimulator.Grpc.LogisticsSimulatorService.LogisticsSimulatorServiceClient>(
                (serviceProvider, options) =>
                {
                    var grpcClientsOptions = serviceProvider.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;

                    options.Address = new Uri(grpcClientsOptions.LogisticSimulatorClientAddress);
                });

        return serviceCollection;
    }

    private static IServiceCollection AddCustomerServiceGrpcClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddGrpcClient<Customers.Grpc.Customers.CustomersClient>(
            (serviceProvider, options) =>
            {
                var grpcClientsOptions = serviceProvider.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;

                options.Address = new Uri(grpcClientsOptions.CustomerServiceClientAddress);
            });

        return serviceCollection;
    }
}