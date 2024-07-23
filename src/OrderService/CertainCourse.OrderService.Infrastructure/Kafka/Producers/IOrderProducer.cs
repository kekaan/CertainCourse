using CertainCourse.OrderService.Application.MessageBrokers.Models;

namespace CertainCourse.OrderService.Infrastructure.Kafka.Producers;

internal interface IOrderProducer
{
    Task ProduceAsync(IReadOnlyCollection<NewOrder> newOrders, CancellationToken cancellationToken);
}