using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Models.Orders;
using CertainCourse.GatewayService.Models.Orders.Dtos;
using CertainCourse.Orders.Grpc;

namespace CertainCourse.GatewayService.Tests.ServicesTests;

public class GrpcOrdersServiceTests
{
    private readonly Mock<Orders.Grpc.Orders.OrdersClient> _mockClient;
    private readonly GrpcOrdersService _service;

    public GrpcOrdersServiceTests()
    {
        _mockClient = new Mock<Orders.Grpc.Orders.OrdersClient>();
        _service = new GrpcOrdersService(_mockClient.Object, Mock.Of<ILogger<GrpcOrdersService>>());
    }

    [Fact]
    public async Task CancelOrderByIdAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new CancelOrderByIdResponse { IsSuccess = true };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _mockClient
            .Setup(x => x.CancelOrderByIdAsync(
                It.IsAny<CancelOrderByIdRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        // Act
        var result = await _service.CancelOrderByIdAsync(123, CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.True(result.Data!.IsSuccess);
    }

    [Fact]
    public async Task GetOrderStateByIdAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new GetOrderStateByIdResponse { State = OrderState.Created };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
        _mockClient.Setup(x => x.GetOrderStateByIdAsync(
                It.IsAny<GetOrderStateByIdRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        // Act
        var result = await _service.GetOrderStateByIdAsync(123, CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.Equal(OrderState.Created.ToString(), result.Data!.OrderState.ToString());
    }

    [Fact]
    public async Task GetRegionsAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new GetRegionsResponse
        {
            Regions =
            {
                new List<Region>
                {
                    new() { Id = 1, Name = "Region1" },
                    new() { Id = 2, Name = "Region2" }
                }
            }
        };

        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
        _mockClient.Setup(x => x.GetRegionsAsync(
                It.IsAny<GetRegionsRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        // Act
        var result = await _service.GetRegionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.Equal(response.Regions[0].Id, result.Data!.Regions.First().Id);
        Assert.Equal(response.Regions[0].Name, result.Data!.Regions.First().Name);
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new GetOrdersResponse
        {
            Orders =
            {
                new Order
                {
                    Id = 1,
                    CustomerFirstName = "John",
                    State = OrderState.Created,
                    TotalAmount = 10,
                    Address = new Address
                    {
                        Apartment = "Apartment",
                        City = "City"
                    }
                }
            },
            NextPageToken = "2"
        };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
        _mockClient.Setup(x => x.GetOrdersAsync(
                It.IsAny<GetOrdersRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        var requestDto = new GetOrdersRequestDto
        (
            365,
            10,
            "2",
            "asc"
        );

        // Act
        var result = await _service.GetOrdersAsync(requestDto, CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.Single(result.Data!.Orders);
        Assert.Equal("2", result.Data!.NextPageToken);
    }

    [Fact]
    public async Task GetOrdersAggregatedByRegionAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new GetOrdersAggregatedByRegionResponse
        {
            OrdersAggregatedByRegion =
            {
                new OrdersAggregatedByRegion
                {
                    Region = "Region1",
                    OrdersCount = 10,
                    OrdersTotalAmount = 1000,
                    OrdersTotalWeight = 500,
                    CustomersCount = 5
                }
            }
        };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
        _mockClient.Setup(x => x.GetOrdersAggregatedByRegionAsync(
                It.IsAny<GetOrdersAggregatedByRegionRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        var requestDto = new GetOrdersAggregatedByRegionRequestDto
        (
            DateTime.UtcNow.AddDays(-1),
            new List<int> { 1, 2, 3 }
        );

        // Act
        var result = await _service.GetOrdersAggregatedByRegionAsync(requestDto, CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.Single(result.Data!);
        var aggregatedResult = result.Data!.First();
        Assert.Equal("Region1", aggregatedResult.Region);
        Assert.Equal(10, aggregatedResult.OrdersCount);
        Assert.Equal(1000, aggregatedResult.OrdersTotalAmount);
        Assert.Equal(500, aggregatedResult.OrdersTotalWeight);
        Assert.Equal(5, aggregatedResult.CustomersCount);
    }

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new GetOrdersByCustomerIdResponse
        {
            Orders =
            {
                new Order
                {
                    Id = 1,
                    CustomerFirstName = "John",
                    State = OrderState.Created,
                    TotalAmount = 10,
                    Address = new Address
                    {
                        Apartment = "Apartment",
                        City = "City"
                    }
                }
            },
            NextPageToken = "2"
        };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
        _mockClient.Setup(x => x.GetOrdersByCustomerIdAsync(
                It.IsAny<GetOrdersByCustomerIdRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        var requestDto = new GetOrdersByCustomerIdRequestDto
        (
            123,
            DateTime.UtcNow.AddDays(-1),
            10,
            "2"
        );

        // Act
        var result = await _service.GetOrdersByCustomerIdAsync(requestDto, CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.Single(result.Data!.Orders);
        Assert.Equal("2", result.Data!.NextPageToken);
    }
}