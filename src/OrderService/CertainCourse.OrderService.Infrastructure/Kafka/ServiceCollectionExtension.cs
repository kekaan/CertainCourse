using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.Infrastructure.Kafka.Consumers;
using CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;
using CertainCourse.OrderService.Infrastructure.Kafka.OutboxMessagesProducing;
using CertainCourse.OrderService.Infrastructure.Kafka.Producers;

namespace CertainCourse.OrderService.Infrastructure.Kafka;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IPreOrderDataProvider, PreOrderDataProvider>()
            .AddSingleton<IOrderEventsDataProvider, OrderEventsDataProvider>()
            .AddSingleton<IOrderProducer, OrderProducer>()
            .AddHostedService<NewPreOrderConsumer>()
            .AddHostedService<OrderEventConsumer>()
            .AddHostedService<NewOrdersOutboxProcessorService>();
    }
}