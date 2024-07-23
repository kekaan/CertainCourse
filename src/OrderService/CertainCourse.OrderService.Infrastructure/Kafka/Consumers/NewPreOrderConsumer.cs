using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.NewPreOrder;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

namespace CertainCourse.OrderService.Infrastructure.Kafka.Consumers;

internal sealed class NewPreOrderConsumer : ConsumerBackgroundService<long, string>
{
    private readonly ILogger<NewPreOrderConsumer> _logger;
    private readonly INewPreOrderHandler _newPreOrderHandler;
    
    public NewPreOrderConsumer(
        ILogger<NewPreOrderConsumer> logger,
        IServiceProvider serviceProvider,
        IPreOrderDataProvider preOrderDataProvider,
        IOptions<OrderServiceKafkaOptions> kafkaSettings) 
        : base(logger, serviceProvider, preOrderDataProvider, kafkaSettings.Value.PreOrderTopic)
    {
        _logger = logger;
        _newPreOrderHandler = Scope.ServiceProvider.GetRequiredService<INewPreOrderHandler>();
    }

    protected override async Task HandleAsync(long key, string? value, CancellationToken cancellationToken)
    {
        await _newPreOrderHandler.Handle(ToPreOrder(key, value), cancellationToken);
        _logger.LogInformation("New order created");
    }
    
    private static NewPreOrderEvent ToPreOrder(long key, string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value was null or empty");
        
        return JsonSerializer.Deserialize<NewPreOrderEvent>(value, _jsonSerializerOptions)
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize order event. Key: {key}, Value: {value}");
    }
}