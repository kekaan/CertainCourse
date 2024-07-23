using Confluent.Kafka;

namespace CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

public interface IKafkaDataProvider<TKey, TValue>
{
    public IConsumer<TKey, TValue> Consumer { get; }
    
    public IProducer<TKey, TValue> Producer { get; }
}