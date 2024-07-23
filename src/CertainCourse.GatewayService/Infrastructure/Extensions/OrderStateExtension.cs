using CertainCourse.GatewayService.Models.Orders.Enums;

namespace CertainCourse.GatewayService.Infrastructure.Extensions;

internal static class OrderStateExtension
{
    public static OrderState ToOrderState(this CertainCourse.Orders.Grpc.OrderState orderState)
    {
        return orderState switch
        {
            Orders.Grpc.OrderState.Created => OrderState.Created,
            Orders.Grpc.OrderState.SentToCustomer => OrderState.SentToCustomer,
            Orders.Grpc.OrderState.Delivered => OrderState.Delivered,
            Orders.Grpc.OrderState.Lost => OrderState.Lost,
            Orders.Grpc.OrderState.Cancelled => OrderState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderState), orderState, null)
        };
    }
}