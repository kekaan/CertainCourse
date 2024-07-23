using Moq;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;
using TestsCommon;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.RepositoriesTests;

public class OrderRepositoryTests
{
    private readonly Mock<IOrderDbRepository> _orderDbRepositoryMock;
    private readonly Mock<IRegionDbRepository> _regionDbRepositoryMock;
    private readonly OrderRepository _orderRepository;

    public OrderRepositoryTests()
    {
        _orderDbRepositoryMock = new Mock<IOrderDbRepository>();
        _regionDbRepositoryMock = new Mock<IRegionDbRepository>();
        _orderRepository = new OrderRepository(_orderDbRepositoryMock.Object, _regionDbRepositoryMock.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrderWithRegion_WhenIncludeSpecificationIsProvided()
    {
        // Arrange
        var orderId = 1;
        var regionId = 1;
        var order = new OrderDal { Id = orderId, RegionId = regionId, CustomerMobileNumber = MobileNumber.Create("799999")};
        var region = new RegionDal { Id = regionId };

        _orderDbRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _regionDbRepositoryMock.Setup(repo => repo.GetByIdAsync(regionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);

        var includeSpecification = new OrderIncludeSpecification(true);

        // Act
        var result = await _orderRepository.GetOrderByIdAsync(orderId, CancellationToken.None, includeSpecification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, (int)result.Id);
        Assert.Equal(regionId, result.RegionId);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCallInsertAsync()
    {
        // Arrange
        var order = TestEntities.Order;

        // Act
        await _orderRepository.CreateOrderAsync(order, false, CancellationToken.None);

        // Assert
        _orderDbRepositoryMock.Verify(
            repo => repo.InsertAsync(
                It.IsAny<OrderDal>(), false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowException_WhenOrderIsAlreadyCancelledOrDelivered()
    {
        // Arrange
        var orderId = 1;
        var order = TestEntities.Order with { State = Domain.ValueObjects.Order.OrderState.Delivered };

        _orderDbRepositoryMock.Setup(repo => repo.GetByIdAsync(
                orderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order.ToDal());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orderRepository.CancelOrderAsync(orderId, CancellationToken.None));
        Assert.Equal($"Order with this id is already delivered or cancelled: {orderId}", exception.Message);
    }
}