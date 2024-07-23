using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Infrastructure.Configuration;

namespace CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

internal sealed class OrderEventsDataProvider : BaseDataProvider, IOrderEventsDataProvider
{
    public OrderEventsDataProvider(ILogger<OrderEventsDataProvider> logger,
        IOptions<OrderServiceKafkaOptions> kafkaSettings) :
        base(logger, kafkaSettings.Value.OrdersEventsConsumerGroupId ?? "order_events_group")
    {
    }
}