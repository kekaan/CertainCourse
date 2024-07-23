using Grpc.Core;
using Moq;
using CertainCourse.Orders.Grpc;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.LogisticSimulatorClient;
using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using CertainCourse.OrderService.Infrastructure.Grpc.Services;
using TestsCommon;
using Xunit;
using OrderState = CertainCourse.Orders.Grpc.OrderState;

namespace CertainCourse.OrderService.Tests.ServicesTests;

public class OrdersServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IRegionRepository> _mockRegionRepository;
    private readonly Mock<ILogisticSimulatorClient> _mockLogisticsSimulatorClient;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly OrdersService _ordersService;
    
    public OrdersServiceTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockRegionRepository = new Mock<IRegionRepository>();
        _mockLogisticsSimulatorClient = new Mock<ILogisticSimulatorClient>();
        
        _ordersService = new OrdersService(
            _mockOrderRepository.Object,
            _mockRegionRepository.Object,
            _mockLogisticsSimulatorClient.Object,
            _mockCustomerRepository.Object
        );
    }

    [Fact]
    public async Task CancelOrderById_OrderNotFound_ThrowsRpcException()
    {
        // Arrange
        var request = new CancelOrderByIdRequest { Id = 1 };
        
        _mockLogisticsSimulatorClient
            .Setup(c => c.CancelOrderAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LogisticSimulatorCancelOrderResult(true, null));
        _mockOrderRepository
            .Setup(r => r.CancelOrderAsync(
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _ordersService.CancelOrderById(request, TestServerCallContext.Create()));
        
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task CancelOrderById_LogisticsSimulatorCancelFails_ReturnsErrorResponse()
    {
        // Arrange
        var request = new CancelOrderByIdRequest { Id = 1 };
        _mockLogisticsSimulatorClient
            .Setup(c => c.CancelOrderAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LogisticSimulatorCancelOrderResult(false, "Failed to cancel"));

        // Act
        var response = await _ordersService.CancelOrderById(request, TestServerCallContext.Create());

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Cancelling order on logistic simulator error: Failed to cancel", response.ErrorMessage);
    }

    [Fact]
    public async Task CancelOrderById_SuccessfulCancellation_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new CancelOrderByIdRequest { Id = 1 };
        
        _mockLogisticsSimulatorClient
            .Setup(c => c.CancelOrderAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LogisticSimulatorCancelOrderResult(true, null));
        _mockOrderRepository
            .Setup(r => r.CancelOrderAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _ordersService.CancelOrderById(request, TestServerCallContext.Create());

        // Assert
        Assert.True(response.IsSuccess);
        Assert.True(string.IsNullOrEmpty(response.ErrorMessage));
    }

    [Fact]
    public async Task GetOrderStateById_OrderNotFound_ThrowsRpcException()
    {
        // Arrange
        var request = new GetOrderStateByIdRequest { Id = 1 };
        _mockOrderRepository
            .Setup(r => r.GetOrderByIdAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<OrderIncludeSpecification>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _ordersService.GetOrderStateById(request, TestServerCallContext.Create()));
        
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetOrderStateById_OrderFound_ReturnsOrderState()
    {
        // Arrange
        var request = new GetOrderStateByIdRequest { Id = 1 };
        _mockOrderRepository
            .Setup(r => r.GetOrderByIdAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<OrderIncludeSpecification>()))
            .ReturnsAsync(TestEntities.Order);

        // Act
        var response = await _ordersService.GetOrderStateById(request, TestServerCallContext.Create());

        // Assert
        Assert.Equal(OrderState.Created, response.State);
    }

    [Fact]
    public async Task GetOrders_RegionNotFound_ThrowsRpcException()
    {
        // Arrange
        var request = new GetOrdersRequest { RegionId = 1 };
        _mockRegionRepository
            .Setup(r => r.IsRegionExistAsync(It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _ordersService.GetOrders(request, TestServerCallContext.Create()));
        
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetOrders_RegionFound_ReturnsOrders()
    {
        // Arrange
        var request = new GetOrdersRequest { RegionId = 1, PageSize = 10, PageToken = "0", OrderBy = "asc" };
        var pagedOrders = new PagedCollection<OrderEntity>(
            new List<OrderEntity> { TestEntities.Order },
            10, 
            "2",
            "3");
        
        _mockRegionRepository
            .Setup(r => r.IsRegionExistAsync(It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _mockOrderRepository
            .Setup(r => r.GetOrdersPagedListAsync(It.IsAny<PaginationSpecification>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<OrderIncludeSpecification>(),
                It.IsAny<IReadOnlyCollection<OrderFilteringSpecification>>(),
                It.IsAny<IReadOnlyCollection<OrderSortingSpecification>>()))
            .ReturnsAsync(pagedOrders);

        // Act
        var response = await _ordersService.GetOrders(request, TestServerCallContext.Create());

        // Assert
        Assert.Single(response.Orders);
        Assert.Equal(12345, response.Orders.First().Id);
        Assert.Equal("3", response.NextPageToken);
    }

    [Fact]
    public async Task GetRegions_ReturnsRegions()
    {
        // Arrange
        var request = new GetRegionsRequest();
        var regions = new List<RegionEntity> { new(new RegionId(1), "Moscow", new Storage(55.700608, 37.588771)) };
        _mockRegionRepository
            .Setup(r => r.GetRegionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(regions);

        // Act
        var response = await _ordersService.GetRegions(request, TestServerCallContext.Create());

        // Assert
        Assert.Single(response.Regions);
        Assert.Equal(1, response.Regions.First().Id);
        Assert.Equal("Moscow", response.Regions.First().Name);
    }

    [Fact]
    public async Task GetOrdersAggregatedByRegion_RegionNotFound_ThrowsRpcException()
    {
        // Arrange
        var request = new GetOrdersAggregatedByRegionRequest { RegionsIds = { 1 } };
        _mockRegionRepository
            .Setup(r => r.IsRegionExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _ordersService.GetOrdersAggregatedByRegion(request, TestServerCallContext.Create()));
        
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetOrdersAggregatedByRegion_RegionsFound_ReturnsAggregatedOrders()
    {
        // Arrange
        var request = new GetOrdersAggregatedByRegionRequest { RegionsIds = { 1 } };
        var orders = new List<OrderEntity>
        {
            TestEntities.Order
        };
        
        _mockRegionRepository
            .Setup(r => r.IsRegionExistAsync(It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _mockOrderRepository
            .Setup(r => r.GetOrdersAsync(It.IsAny<CancellationToken>(),
                It.IsAny<OrderIncludeSpecification>(),
                It.IsAny<IReadOnlyCollection<OrderFilteringSpecification>>(),
                null))
            .ReturnsAsync(orders);

        // Act
        var response = await _ordersService.GetOrdersAggregatedByRegion(request, TestServerCallContext.Create());

        // Assert
        Assert.Single(response.OrdersAggregatedByRegion);
        Assert.Equal("Moscow", response.OrdersAggregatedByRegion.First().Region);
        Assert.Equal(1, response.OrdersAggregatedByRegion.First().CustomersCount);
    }

    [Fact]
    public async Task GetOrdersByCustomerId_CustomerFound_ReturnsOrders()
    {
        // Arrange
        var request = new GetOrdersByCustomerIdRequest { CustomerId = 1, PageSize = 10, PageToken = "0" };
        var pagedOrders = new PagedCollection<OrderEntity>(
            new List<OrderEntity> { TestEntities.Order }, 10, "0", "1");
        
        _mockCustomerRepository
            .Setup(x => x.GetCustomerByIdAsync(
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestEntities.Customer);
        
        _mockOrderRepository
            .Setup(r => r.GetOrdersPagedListAsync(It.IsAny<PaginationSpecification>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<OrderIncludeSpecification>(),
                It.IsAny<IReadOnlyCollection<OrderFilteringSpecification>>(),
                null))
            .ReturnsAsync(pagedOrders);

        // Act
        var response = await _ordersService.GetOrdersByCustomerId(request, TestServerCallContext.Create());

        // Assert
        Assert.Single(response.Orders);
        Assert.Equal(12345, response.Orders.First().Id);
        Assert.Equal("1", response.NextPageToken);
    }
}
