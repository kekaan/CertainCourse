using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using CertainCourse.Customers.Grpc;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Models.Customers;

namespace CertainCourse.GatewayService.Tests.ServicesTests;

public class GrpcCustomersServiceTests
{
    private readonly Mock<Customers.Grpc.Customers.CustomersClient> _mockClient;
    private readonly GrpcCustomersService _service;

    public GrpcCustomersServiceTests()
    {
        _mockClient = new Mock<Customers.Grpc.Customers.CustomersClient>();
        _service = new GrpcCustomersService(_mockClient.Object, Mock.Of<ILogger<GrpcCustomersService>>());
    }

    [Fact]
    public async Task CancelOrderByIdAsync_ReturnsSuccessResult_WhenGrpcCallSucceeds()
    {
        // Arrange
        var response = new GetCustomersResponse
        {
            Customers =
            {
                new Customer
                {
                    Id = 1,
                    FirstName = "Vasya",
                    LastName = "Pupkin",
                    DefaultAddress = new Address { Apartment = "Apartment", City = "City" },
                    Addressed =
                    {
                        new Address { Apartment = "Apartment", City = "City" }
                    }
                }
            }
        };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => [],
            () => { });

        _mockClient
            .Setup(x => x.GetCustomersAsync(
                It.IsAny<GetCustomersRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(fakeCall);

        // Act
        var result = await _service.GetCustomersAsync(CancellationToken.None);

        // Assert
        Assert.Equal(CallResultStatusCode.OK, result.StatusCode);
        Assert.Single(result.Data!.Customers);
        Assert.Equal("Vasya", result.Data!.Customers.First().FirstName);
        Assert.Equal("Pupkin", result.Data!.Customers.First().LastName);
    }
}