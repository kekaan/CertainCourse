namespace CertainCourse.OrderService.BackgroundJobs;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddHostedService<ServiceDiscoveryConsumerHostedService>();
    }
}