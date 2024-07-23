namespace CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

public interface IOrderEventsDataProvider : IKafkaDataProvider<long, string>
{
}