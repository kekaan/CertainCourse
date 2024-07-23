using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CertainCourse.OrderService.Infrastructure.Redis;

public static class ServiceCollectionExtension
{
    private const string CERTAIN_COURSE_REDIS_CONNECT = "CERTAIN_COURSE_REDIS_CONNECT";
    
    public static IServiceCollection AddRedis(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IConnectionMultiplexer>(
            _ =>
            { 
                var redisConnect= Environment.GetEnvironmentVariable(CERTAIN_COURSE_REDIS_CONNECT) 
                                  ?? throw new ArgumentNullException(
                                      CERTAIN_COURSE_REDIS_CONNECT, 
                                      $"{CERTAIN_COURSE_REDIS_CONNECT} is not set");
                
                var connection = ConnectionMultiplexer.Connect(redisConnect);

                return connection;
            });

        return serviceCollection;
    }
}