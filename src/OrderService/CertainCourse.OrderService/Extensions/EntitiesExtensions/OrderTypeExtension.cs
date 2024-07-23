using CertainCourse.OrderService.Domain.ValueObjects.Order;
using GrpcOrderType = CertainCourse.Orders.Grpc.OrderType;

namespace CertainCourse.OrderService.Extensions.EntitiesExtensions;

internal static class OrderTypeExtension
{
    public static GrpcOrderType ToProto(this OrderType orderState)
    {
        return orderState switch
        {
            OrderType.Mobile => GrpcOrderType.Mobile,
            OrderType.Web => GrpcOrderType.Web,
            OrderType.Api => GrpcOrderType.Api,
            _ => throw new ArgumentOutOfRangeException(nameof(orderState), orderState, null)
        };
    }
}