namespace CertainCourse.OrderService.Infrastructure.Configuration;

public sealed record OrderServiceKafkaOptions
{
    public string PreOrderTopic { get; set; } = null!;
    public string NewOrderTopic { get; set; } = null!;
    public string OrdersEventsTopic { get; set; } = null!;
    public string? PreOrdersConsumerGroupId { get; set; }
    public string? OrdersEventsConsumerGroupId { get; set; }
}