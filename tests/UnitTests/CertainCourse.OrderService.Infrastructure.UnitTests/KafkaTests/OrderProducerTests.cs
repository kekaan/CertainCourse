using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Moq;
using CertainCourse.OrderService.Application.MessageBrokers.Models;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.Kafka.DataProviders;
using CertainCourse.OrderService.Infrastructure.Kafka.Producers;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.KafkaTests;

public class OrderProducerTests
{
    private readonly Mock<IProducer<long, string>> _mockProducer;
    private readonly OrderProducer _orderProducer;

    public OrderProducerTests()
    {
        Mock<IPreOrderDataProvider> mockPreOrderDataProvider = new();
        _mockProducer = new Mock<IProducer<long, string>>();

        mockPreOrderDataProvider.Setup(p => p.Producer).Returns(_mockProducer.Object);

        IOptions<OrderServiceKafkaOptions> mockKafkaSettings = Options.Create(new OrderServiceKafkaOptions
        {
            NewOrderTopic = "test-topic"
        });

        _orderProducer = new OrderProducer(mockPreOrderDataProvider.Object, mockKafkaSettings);
    }

    [Fact]
    public async Task ProduceAsync_CreatesAndSendsMessages()
    {
        // Arrange
        var newOrders = new List<NewOrder>
        {
            new(OrderId: 1),
            new(OrderId: 2)
        };

        _mockProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<long, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<long, string>());

        // Act
        await _orderProducer.ProduceAsync(newOrders, CancellationToken.None);

        // Assert
        _mockProducer.Verify(p => p.ProduceAsync(
            "test-topic",
            It.IsAny<Message<long, string>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(newOrders.Count));
    }

    [Fact]
    public async Task ProduceAsync_HandlesCancellation()
    {
        // Arrange
        var newOrders = new List<NewOrder>
        {
            new(OrderId: 1),
            new(OrderId: 2)
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _orderProducer.ProduceAsync(newOrders, cts.Token));
    }

    [Fact]
    public async Task ProduceAsync_SerializesOrderCorrectly()
    {
        // Arrange
        var newOrder = new NewOrder(OrderId: 1);

        _mockProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<long, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Message<long, string>, CancellationToken>((_, message, _) =>
            {
                // Assert
                Assert.Equal(newOrder.OrderId, message.Key);
                Assert.Equal(JsonSerializer.Serialize(newOrder), message.Value);
            })
            .ReturnsAsync(new DeliveryResult<long, string>());

        // Act
        await _orderProducer.ProduceAsync(new[] { newOrder }, CancellationToken.None);
    }
}