using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.MessageBrokers.Models;
using CertainCourse.OrderService.Infrastructure.Kafka.Producers;

namespace CertainCourse.OrderService.Infrastructure.Kafka.OutboxMessagesProducing;

internal sealed class NewOrdersOutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOrderProducer _producer;
    private readonly ILogger<NewOrdersOutboxProcessorService> _logger;

    public NewOrdersOutboxProcessorService(IServiceScopeFactory serviceScopeFactory, IOrderProducer producer,
        ILogger<NewOrdersOutboxProcessorService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<INewOrdersOutboxRepository>();

                var unprocessedNewOrdersMessages =
                    await outboxRepository.GetUnprocessedMessagesAsync(cancellationToken, type: nameof(NewOrder));

                IEnumerable<(long MessageId, Application.MessageBrokers.Models.NewOrder NewOrder)> unsentNewOrders = unprocessedNewOrdersMessages
                    .Select(m => (m.Id, JsonSerializer.Deserialize<NewOrder>(m.Payload)))
                    .OfType<(long, NewOrder)>();

                var unsentNewOrdersArray =
                    unsentNewOrders as (long MessageId, NewOrder NewOrder)[] ?? unsentNewOrders.ToArray();

                await _producer.ProduceAsync(
                    unsentNewOrdersArray.Select(m => m.NewOrder).ToArray(),
                    cancellationToken);

                foreach (var messageId in unsentNewOrdersArray.Select(m => m.MessageId))
                {
                    await outboxRepository.MarkAsProcessedAsync(messageId, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}