using FluentAssertions;
using Grpc.Net.Client;
using CertainCourse.Orders.Grpc;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using OrdersClient = CertainCourse.Orders.Grpc.Orders.OrdersClient;
using OrderState = CertainCourse.Orders.Grpc.OrderState;

namespace CertainCourse.OrderService.IntegrationTests.GrpcServices;

public sealed class OrdersServiceTests : IClassFixture<TestApplication>
{
    private readonly OrdersClient _client;

    public OrdersServiceTests(TestApplication testApplication)
    {
        var httpClient = testApplication.CreateClient();

        var grpcChannel = GrpcChannel.ForAddress(
            httpClient.BaseAddress ?? throw new ArgumentNullException(nameof(httpClient.BaseAddress),
                "HttpClient base address cannot be null"),
            new GrpcChannelOptions
            {
                HttpClient = httpClient
            });

        _client = new OrdersClient(grpcChannel);
    }

    [Theory]
    [InlineData(71306109, OrderState.Created)]
    [InlineData(51505992, OrderState.Delivered)]
    [InlineData(89677142, OrderState.SentToCustomer)]
    public async Task CancelOrderById_Succeeds(int orderId, OrderState expectedState)
    {
        // Arrange
        var request = new GetOrderStateByIdRequest
        {
            Id = orderId
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var response =
            await _client.GetOrderStateByIdAsync(request, cancellationToken: cancellationTokenSource.Token);

        // Assert
        response.State.Should().Be(expectedState);
    }

    [Fact]
    public async Task GetRegions_Succeeds()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var response =
            await _client.GetRegionsAsync(new GetRegionsRequest(), cancellationToken: cancellationTokenSource.Token);

        // Assert
        response.Regions.Should().NotBeNull();
        response.Regions.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(71306109, OrderState.Created)]
    [InlineData(51505992, OrderState.Delivered)]
    [InlineData(89677142, OrderState.SentToCustomer)]
    public async Task GetOrderStateById_Succeeds(int orderId, OrderState expectedState)
    {
        // Arrange
        var request = new GetOrderStateByIdRequest
        {
            Id = orderId
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var response =
            await _client.GetOrderStateByIdAsync(request, cancellationToken: cancellationTokenSource.Token);

        // Assert
        response.State.Should().Be(expectedState);
    }


    [Theory]
    [InlineData(1, 10, "", "asc", "CgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "Moscow")]
    [InlineData(1, 10, "", "desc", "CgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "Moscow")]
    [InlineData(1, 10, "CgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "asc",
        "FAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "Moscow")]
    [InlineData(1, 20, "", "asc", "FAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "Moscow")]
    [InlineData(2, 10, "", "asc", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", "StPetersburg")]
    public async Task GetOrders_Succeeds(int regionId, int pageSize, string? pageToken, string orderBy,
        string expectedNextPageToken, string expectedRegion)
    {
        // Arrange
        var request = new GetOrdersRequest
        {
            RegionId = regionId,
            PageSize = pageSize,
            PageToken = pageToken,
            OrderBy = orderBy
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var response = await _client.GetOrdersAsync(request, cancellationToken: cancellationTokenSource.Token);

        // Assert
        response.Orders.Should().NotBeNull();
        response.Orders.Should().HaveCountLessThanOrEqualTo(pageSize);
        if (response.Orders.Count > 0)
            response.Orders.Should().OnlyContain(o => o.Region == expectedRegion);
        response.NextPageToken.Should().NotBeNull();
        response.NextPageToken.Should().Be(expectedNextPageToken);

        if (orderBy is "asc")
        {
            response.Orders.Should().BeInAscendingOrder(x => x.Region);
        }
        else
        {
            response.Orders.Should().BeInDescendingOrder(x => x.Region);
        }
    }

    [Theory]
    [InlineData(1234, new[] { 1 }, new[] { "Moscow" })]
    public async Task GetOrdersAggregatedByRegion_Succeeds(long startTime, int[] regionsIds, string[] expectedRegions)
    {
        // Arrange
        var request = new GetOrdersAggregatedByRegionRequest
        {
            StartTime = startTime,
            RegionsIds = { regionsIds }
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var response =
            await _client.GetOrdersAggregatedByRegionAsync(request, cancellationToken: cancellationTokenSource.Token);

        // Assert
        response.OrdersAggregatedByRegion.Should().OnlyContain(o => expectedRegions.Contains(o.Region));
        response.OrdersAggregatedByRegion.Should().HaveCount(regionsIds.Length);
    }

    [Theory]
    [InlineData(11, 123, 10, "", "Дарья", "Яковлева", "+ 7 909 111 00 22")]
    public async Task GetOrdersByCustomerIdAsync_Succeeds(int customerId, long startTime, int pageSize,
        string? pageToken, string expectedFirstName, string expectedLastName, string expectedNumber)
    {
        // Arrange
        var request = new GetOrdersByCustomerIdRequest
        {
            CustomerId = customerId,
            StartTime = startTime,
            PageSize = pageSize,
            PageToken = pageToken,
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var response =
            await _client.GetOrdersByCustomerIdAsync(request, cancellationToken: cancellationTokenSource.Token);

        // Assert
        response.Orders.Should().OnlyContain(o => o.CustomerFirstName == expectedFirstName);
        response.Orders.Should().OnlyContain(o => o.CustomerLastName == expectedLastName);
        response.Orders.Should().OnlyContain(o => o.CustomerMobileNumber == expectedNumber);
    }
}