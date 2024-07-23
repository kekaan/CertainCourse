using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

namespace CertainCourse.OrderService.Infrastructure.Kafka.Consumers;

internal abstract class ConsumerBackgroundService<TKey, TValue> : BackgroundService
{
    private readonly string _topicName;
    private readonly ILogger _logger;
    private readonly IKafkaDataProvider<TKey, TValue> _kafkaDataProvider;
    protected readonly IServiceScope Scope;

    protected static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    protected ConsumerBackgroundService(
        ILogger logger,
        IServiceProvider serviceProvider,
        IKafkaDataProvider<TKey, TValue> kafkaDataProvider,
        string topicName)
    {
        _logger = logger;
        _kafkaDataProvider = kafkaDataProvider;
        _topicName = topicName;

        Scope = serviceProvider.CreateScope();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        _kafkaDataProvider.Consumer.Subscribe(_topicName);

        _logger.LogInformation($"Start consumer topic {_topicName}");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ConsumeAsync(stoppingToken);
        }

        _kafkaDataProvider.Consumer.Unsubscribe();

        _logger.LogInformation($"Stop consumer topic {_topicName}");
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        ConsumeResult<TKey, TValue>? message = null;

        try
        {
            message = _kafkaDataProvider.Consumer.Consume(TimeSpan.FromMilliseconds(100));

            if (message is null)
            {
                await Task.Delay(100, cancellationToken);
                return;
            }

            if (TryGetMessageValues(message, out var key, out var value))
                await HandleAsync(key, value, cancellationToken);

            _kafkaDataProvider.Consumer.Commit();
        }
        catch (Exception exc)
        {
            var key = message is not null ? message.Message.Key?.ToString() : "No key";
            var value = message is not null ? message.Message.Value?.ToString() : "No value";

            _logger.LogError(exc, "Error process message Key:{Key} Value:{Value}", key, value);
        }
    }

    private bool TryGetMessageValues(ConsumeResult<TKey, TValue> message, out TKey? key, out TValue? value)
    {
        if (message.Message is null)
        {
            _logger.LogWarning($"Message was null. {message.Topic}");
            
            key = default;
            value = default;
            
            return false;
        }

        key = message.Message!.Key;
        value = message.Message!.Value;

        return true;
    }

    protected abstract Task HandleAsync(TKey? key, TValue? message, CancellationToken cancellationToken);

    public override void Dispose()
    {
        Scope.Dispose();
        base.Dispose();
    }
}