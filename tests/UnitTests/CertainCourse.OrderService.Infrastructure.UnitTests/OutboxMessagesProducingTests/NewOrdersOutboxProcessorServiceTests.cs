using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Mock;
using Moq;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Application.MessageBrokers.Models;
using CertainCourse.OrderService.Infrastructure.Kafka.OutboxMessagesProducing;
using CertainCourse.OrderService.Infrastructure.Kafka.Producers;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.OutboxMessagesProducingTests;

public class NewOrdersOutboxProcessorServiceTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IOrderProducer> _mockOrderProducer;
    private readonly Mock<ILogger<NewOrdersOutboxProcessorService>> _mockLogger;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<INewOrdersOutboxRepository> _mockOutboxRepository;

    private readonly NewOrdersOutboxProcessorService _service;

    public NewOrdersOutboxProcessorServiceTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockOrderProducer = new Mock<IOrderProducer>();
        _mockLogger = new Mock<ILogger<NewOrdersOutboxProcessorService>>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockOutboxRepository = new Mock<INewOrdersOutboxRepository>();

        _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(INewOrdersOutboxRepository)))
            .Returns(_mockOutboxRepository.Object);

        _service = new NewOrdersOutboxProcessorService(
            _mockServiceScopeFactory.Object,
            _mockOrderProducer.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CallsGetUnprocessedMessagesAsync()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(1500); // Cancel after 100ms to stop the infinite loop

        _mockOutboxRepository
            .Setup(repo => repo.GetUnprocessedMessagesAsync(
                    It.IsAny<CancellationToken>(), 
                    It.IsAny<string>()))
            .ReturnsAsync(new List<NewOrdersOutboxMessage>());

        await _service.StartAsync(cts.Token);

        await Task.Delay(1000, cts.Token);
        _mockOutboxRepository.Verify(repo =>
            repo.GetUnprocessedMessagesAsync(
                It.IsAny<CancellationToken>(), 
                nameof(NewOrder)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesUnprocessedMessages()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(1500);

        var outboxMessages = new List<NewOrdersOutboxMessage>
        {
            new(1,
                nameof(NewOrder),
                JsonSerializer.Serialize(new NewOrder(1)),
                DateTime.UtcNow,
                null)
        };

        _mockOutboxRepository
            .Setup(repo => repo.GetUnprocessedMessagesAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(outboxMessages);

        await _service.StartAsync(cts.Token);

        await Task.Delay(1000, cts.Token);
        _mockOrderProducer.Verify(producer => producer.ProduceAsync(
                It.IsAny<NewOrder[]>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        _mockOutboxRepository.Verify(repo => repo.MarkAsProcessedAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_LogsErrorOnException()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(1500); // Cancel after 100ms to stop the infinite loop

        _mockOutboxRepository
            .Setup(repo => repo.GetUnprocessedMessagesAsync(It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        await _service.StartAsync(cts.Token);
        await Task.Delay(1000, cts.Token);

        _mockLogger.VerifyLog().ErrorWasCalled().MessageEquals("Error processing outbox messages");
    }
}