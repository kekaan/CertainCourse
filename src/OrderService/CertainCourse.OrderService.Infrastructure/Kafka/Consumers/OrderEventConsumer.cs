using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.OrderStateChanged;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;

namespace CertainCourse.OrderService.Infrastructure.Kafka.Consumers;

internal sealed class OrderEventConsumer : ConsumerBackgroundService<long, string>
{
    private readonly ILogger<OrderEventConsumer> _logger;
    private readonly IOrderStateChangedHandler _orderRegistrationHandler;

    public OrderEventConsumer(
        ILogger<OrderEventConsumer> logger,
        IServiceProvider serviceProvider,
        IOrderEventsDataProvider orderEventsDataProvider,
        IOptions<OrderServiceKafkaOptions> kafkaSettings)
        : base(logger, serviceProvider, orderEventsDataProvider, kafkaSettings.Value.OrdersEventsTopic)
    {
        _logger = logger;
        _orderRegistrationHandler = Scope.ServiceProvider.GetRequiredService<IOrderStateChangedHandler>();
    }

    protected override async Task HandleAsync(long key, string? value, CancellationToken cancellationToken)
    {
        await _orderRegistrationHandler.Handle(ToOrderEvent(key, value), cancellationToken);
        _logger.LogInformation($"Order state updated");
    }

    private OrderStateChangedEvent ToOrderEvent(long key, string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value was null or empty");
        
        return JsonSerializer.Deserialize<OrderStateChangedEvent>(value, _jsonSerializerOptions)
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize order event. Key: {key}, Value: {value}");
    }
}