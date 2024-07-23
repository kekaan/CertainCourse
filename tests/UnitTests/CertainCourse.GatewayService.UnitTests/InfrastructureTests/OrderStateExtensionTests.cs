using CertainCourse.GatewayService.Infrastructure.Extensions;
using CertainCourse.GatewayService.Models.Orders.Enums;

namespace CertainCourse.GatewayService.Tests.InfrastructureTests;

public class OrderStateExtensionTests
{
    [Theory]
    [InlineData(Orders.Grpc.OrderState.Created, OrderState.Created)]
    [InlineData(Orders.Grpc.OrderState.SentToCustomer, OrderState.SentToCustomer)]
    [InlineData(Orders.Grpc.OrderState.Delivered, OrderState.Delivered)]
    public void ToOrderState_ShouldConvertCorrectly(CertainCourse.Orders.Grpc.OrderState grpcOrderState, OrderState expectedOrderState)
    {
        // Act
        OrderState result = grpcOrderState.ToOrderState();

        // Assert
        Assert.Equal(expectedOrderState, result);
    }
}