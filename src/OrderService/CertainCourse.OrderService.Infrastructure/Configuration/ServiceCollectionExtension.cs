using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CertainCourse.OrderService.Infrastructure.Configuration;

public static class ServiceCollectionExtension
{
    private const string CERTAIN_COURSE_DATABASE_MIGRATE_NEEDED = "CERTAIN_COURSE_DATABASE_MIGRATE_NEEDED";
    private const string CERTAIN_COURSE_KAFKA_BROKERS = "CERTAIN_COURSE_KAFKA_BROKERS";
    private const string CERTAIN_COURSE_KAFKA_PRE_ORDER_TOPIC = "CERTAIN_COURSE_KAFKA_PRE_ORDER_TOPIC";
    private const string CERTAIN_COURSE_KAFKA_NEW_ORDER_TOPIC = "CERTAIN_COURSE_KAFKA_NEW_ORDER_TOPIC";
    private const string CERTAIN_COURSE_KAFKA_ORDER_EVENTS_TOPIC = "CERTAIN_COURSE_KAFKA_ORDER_EVENTS_TOPIC";

    private const string CERTAIN_COURSE_KAFKA_PRE_ORDER_CONSUMER_GROUP_ID =
        "CERTAIN_COURSE_KAFKA_PRE_ORDER_CONSUMER_GROUP_ID";

    private const string CERTAIN_COURSE_KAFKA_ORDERS_EVENTS_CONSUMER_GROUP_ID =
        "CERTAIN_COURSE_KAFKA_ORDERS_EVENTS_CONSUMER_GROUP_ID";

    private const string CONNECTION_STRING_KEY = "OrderServiceDatabase";
    private const string CERTAIN_COURSE_REDIS_DATABASE = "CERTAIN_COURSE_REDIS_DATABASE";
    private const string CERTAIN_COURSE_CUSTOMER_ADDRESS = "CERTAIN_COURSE_CUSTOMER_ADDRESS";
    private const string CERTAIN_COURSE_LOGISTIC_SIMULATOR_ADDRESS = "CERTAIN_COURSE_LOGISTIC_SIMULATOR_ADDRESS";
    private const string CERTAIN_COURSE_SD_ADDRESS = "CERTAIN_COURSE_SD_ADDRESS";

    public static IServiceCollection AddDefinedOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddMigrationOptions(configuration)
            // .AddDatabaseOptions(configuration)
            .AddKafkaOptions(configuration)
            .AddOrderServiceKafkaOptions(configuration)
            .AddRedisOptions()
            .AddShardDbOptions(configuration)
            .AddGrpcClientsOptions(configuration);

        return serviceCollection;
    }

    private static IServiceCollection AddShardDbOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection.Configure<ShardDbOptions>(configuration.GetSection(nameof(ShardDbOptions)));
    }

    private static IServiceCollection AddRedisOptions(this IServiceCollection serviceCollection)
    {
        return serviceCollection.Configure<RedisOptions>(o =>
        {
            o.Server = Convert.ToInt32(Environment.GetEnvironmentVariable(CERTAIN_COURSE_REDIS_DATABASE)
                                       ?? throw new ArgumentNullException(
                                           CERTAIN_COURSE_REDIS_DATABASE,
                                           $"{CERTAIN_COURSE_REDIS_DATABASE} is not set"));
        });
    }

    private static IServiceCollection AddOrderServiceKafkaOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection.Configure<OrderServiceKafkaOptions>(
            o =>
            {
                o.PreOrderTopic = configuration.GetValue<string>(CERTAIN_COURSE_KAFKA_PRE_ORDER_TOPIC)
                                  ?? throw new ArgumentException($"{CERTAIN_COURSE_KAFKA_PRE_ORDER_TOPIC} not set");
                o.NewOrderTopic = configuration.GetValue<string>(CERTAIN_COURSE_KAFKA_NEW_ORDER_TOPIC)
                                  ?? throw new ArgumentException($"{CERTAIN_COURSE_KAFKA_NEW_ORDER_TOPIC} not set");
                o.OrdersEventsTopic = configuration.GetValue<string>(CERTAIN_COURSE_KAFKA_ORDER_EVENTS_TOPIC)
                                      ?? throw new ArgumentException($"{CERTAIN_COURSE_KAFKA_ORDER_EVENTS_TOPIC} not set");

                o.PreOrdersConsumerGroupId =
                    configuration.GetValue<string>(CERTAIN_COURSE_KAFKA_PRE_ORDER_CONSUMER_GROUP_ID);
                o.OrdersEventsConsumerGroupId =
                    configuration.GetValue<string>(CERTAIN_COURSE_KAFKA_ORDERS_EVENTS_CONSUMER_GROUP_ID);
            });
    }

    private static IServiceCollection AddKafkaOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection.Configure<KafkaOptions>(o =>
        {
            o.Servers = configuration.GetValue<string>(CERTAIN_COURSE_KAFKA_BROKERS);
        });
    }

    private static IServiceCollection AddMigrationOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection.Configure<MigrationOptions>(
            o =>
            {
                o.MigrateNeeded = configuration.GetValue<bool>(CERTAIN_COURSE_DATABASE_MIGRATE_NEEDED);
            });
    }

    private static IServiceCollection AddDatabaseOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(CONNECTION_STRING_KEY);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"Connection string is not set");
        }

        return serviceCollection.Configure<DatabaseOptions>(o =>
        {
            o.ConnectionString = connectionString;
        });
    }

    private static IServiceCollection AddGrpcClientsOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var customerClientAddress = configuration.GetValue<string>(CERTAIN_COURSE_CUSTOMER_ADDRESS);

        if (string.IsNullOrEmpty(customerClientAddress))
        {
            throw new ArgumentException(
                $"Требуется указать переменную окружения {CERTAIN_COURSE_CUSTOMER_ADDRESS} или она пустая");
        }

        var logisticSimulatorClientAddress = configuration.GetValue<string>(CERTAIN_COURSE_LOGISTIC_SIMULATOR_ADDRESS);

        if (string.IsNullOrEmpty(logisticSimulatorClientAddress))
        {
            throw new ArgumentException(
                $"Требуется указать переменную окружения {CERTAIN_COURSE_LOGISTIC_SIMULATOR_ADDRESS} или она пустая");
        }

        var serviceDiscoveryClientAddress = configuration.GetValue<string>(CERTAIN_COURSE_SD_ADDRESS);

        if (string.IsNullOrEmpty(serviceDiscoveryClientAddress))
        {
            throw new ArgumentException(
                $"Требуется указать переменную окружения {CERTAIN_COURSE_SD_ADDRESS} или она пустая");
        }
        
        return serviceCollection.Configure<GrpcClientsOptions>(o =>
        {
            o.LogisticSimulatorClientAddress = logisticSimulatorClientAddress;
            o.CustomerServiceClientAddress = customerClientAddress;
            o.ServiceDiscoveryClientAddress = serviceDiscoveryClientAddress;
        });
    }
}