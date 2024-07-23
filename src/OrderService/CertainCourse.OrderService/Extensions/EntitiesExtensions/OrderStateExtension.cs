using CertainCourse.OrderService.Domain.ValueObjects.Order;
using GrpcOrderState = CertainCourse.Orders.Grpc.OrderState;

namespace CertainCourse.OrderService.Extensions.EntitiesExtensions;

internal static class OrderStateExtension
{
    public static GrpcOrderState ToOrderStateProto(this OrderState orderState)
    {
        return orderState switch
        {
            OrderState.Created => GrpcOrderState.Created,
            OrderState.SentToCustomer => GrpcOrderState.SentToCustomer,
            OrderState.Delivered => GrpcOrderState.Delivered,
            OrderState.Lost => GrpcOrderState.Lost,
            OrderState.Cancelled => GrpcOrderState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderState), orderState, null)
        };
    }
}