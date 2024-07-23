using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Infrastructure.Configuration;

namespace CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

internal sealed class PreOrderDataProvider : BaseDataProvider, IPreOrderDataProvider
{
    public PreOrderDataProvider(ILogger<PreOrderDataProvider> logger,
        IOptions<OrderServiceKafkaOptions> kafkaSettings) :
        base(logger, kafkaSettings.Value.PreOrdersConsumerGroupId ?? "pre_orders_group")
    {
    }
}