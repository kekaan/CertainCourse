using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

internal abstract class BaseDataProvider : IKafkaDataProvider<long, string>
{
    private const string KAFKA_BROKERS = "CERTAIN_COURSE_KAFKA_BROKERS";
    
    public IConsumer<long, string> Consumer { get; }
    public IProducer<long, string> Producer { get; }
    
    public BaseDataProvider(ILogger<BaseDataProvider> logger, string kafkaGroupId)
    {
        var bootstrapServers = Environment.GetEnvironmentVariable(KAFKA_BROKERS);

        var consumerConfig = new ConsumerConfig
        {
            GroupId = kafkaGroupId,
            BootstrapServers = bootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        var consumerBuilder = new ConsumerBuilder<long, string>(consumerConfig);

        Consumer = consumerBuilder
            .SetErrorHandler((_, error) => logger.LogError(error.Reason))
            .SetLogHandler((_, message) => logger.LogInformation(message.Message))
            .Build();

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Partitioner = Partitioner.Consistent
        };

        var producerBuilder = new ProducerBuilder<long, string>(producerConfig);

        Producer = producerBuilder
            .SetErrorHandler((_, error) => logger.LogError(error.Reason))
            .SetLogHandler((_, message) => logger.LogInformation(message.Message))
            .Build();
    }
}