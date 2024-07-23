using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.MessageBrokers.Models;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

namespace CertainCourse.OrderService.Infrastructure.Kafka.Producers;

internal sealed class OrderProducer : IOrderProducer
{
    private readonly IPreOrderDataProvider _preOrderDataProvider;
    private readonly string _topicName;

    public OrderProducer(IPreOrderDataProvider preOrderDataProvider, IOptions<OrderServiceKafkaOptions> kafkaSettings)
    {
        _preOrderDataProvider = preOrderDataProvider;
        _topicName = kafkaSettings.Value.NewOrderTopic;
    }

    public async Task ProduceAsync(IReadOnlyCollection<NewOrder> newOrders, CancellationToken cancellationToken)
    {
        await Task.Yield();

        var tasks = new List<Task<DeliveryResult<long, string>>>(newOrders.Count);

        foreach (var newOrder in newOrders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = _preOrderDataProvider.Producer.ProduceAsync(
                _topicName,
                ToMessage(newOrder),
                cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }
    
    private static Message<long, string> ToMessage(NewOrder order)
    {
        return new Message<long, string>
        {
            Key = order.OrderId,
            Value = JsonSerializer.Serialize(order)
        };
    }
}