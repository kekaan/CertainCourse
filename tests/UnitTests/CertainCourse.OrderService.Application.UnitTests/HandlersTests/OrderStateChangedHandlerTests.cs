using Moq;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.OrderStateChanged;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain;
using TestsCommon;
using Xunit;

namespace CertainCourse.OrderService.Application.UnitTests.HandlersTests;

public class OrderStateChangedHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly OrderStateChangedHandler _handler;

    public OrderStateChangedHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _handler = new OrderStateChangedHandler(_mockOrderRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenOrderStateUpdated()
    {
        // Arrange
        var orderId = 1;
        var orderEvent = new OrderStateChangedEvent(orderId, Application.OrderStateChanged.OrderState.Created, DateTime.UtcNow);

        var cancellationToken = CancellationToken.None;

        _mockOrderRepository
            .Setup(x => x.GetOrderByIdAsync(
                orderId,
                cancellationToken,
                It.IsAny<OrderIncludeSpecification>()))
            .ReturnsAsync(TestEntities.Order);

        _mockOrderRepository
            .Setup(x => x.UpdateOrderAsync(It.IsAny<OrderEntity>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(orderEvent, cancellationToken);

        // Assert
        Assert.True(result.Success);
        _mockOrderRepository.Verify(x => x.GetOrderByIdAsync(
            orderId,
            cancellationToken,
            It.IsAny<OrderIncludeSpecification>()), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateOrderAsync(It.IsAny<OrderEntity>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowOrderEventException_OnException()
    {
        // Arrange
        var orderId = 1;
        var orderEvent = new OrderStateChangedEvent(orderId, Application.OrderStateChanged.OrderState.Created, DateTime.UtcNow);

        var cancellationToken = CancellationToken.None;

        _mockOrderRepository
            .Setup(x => x.GetOrderByIdAsync(
                orderId,
                cancellationToken,
                It.IsAny<OrderIncludeSpecification>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act & Assert
        await Assert.ThrowsAsync<OrderStateChangedException>(() => _handler.Handle(orderEvent, cancellationToken));

        _mockOrderRepository.Verify(x => x.GetOrderByIdAsync(
            orderId,
            cancellationToken,
            It.IsAny<OrderIncludeSpecification>()), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateOrderAsync(It.IsAny<OrderEntity>(), cancellationToken), Times.Never);
    }
}