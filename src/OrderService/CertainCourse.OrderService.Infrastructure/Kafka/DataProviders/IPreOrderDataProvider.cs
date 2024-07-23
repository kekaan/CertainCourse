namespace CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

public interface IPreOrderDataProvider : IKafkaDataProvider<long, string>
{
    
}