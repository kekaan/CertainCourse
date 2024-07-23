using CertainCourse.OrderService.Infrastructure.Grpc.Interceptors;

namespace CertainCourse.OrderService.Infrastructure.Grpc;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGrpcServers(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddGrpc(options => options.Interceptors.Add<ServerLoggerInterceptor>());
        serviceCollection.AddGrpcReflection();

        return serviceCollection;
    }
}