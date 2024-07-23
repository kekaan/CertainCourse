using CertainCourse.OrderService.Domain.ValueObjects.Order;
using MessageBrokerOrderType = CertainCourse.OrderService.Application.NewPreOrder.OrderSource;

namespace CertainCourse.OrderService.Application.Extensions;

internal static class OrderTypeExtension
{
    public static OrderType ToDomain(this MessageBrokerOrderType orderType)
    {
        return orderType switch
        {
            MessageBrokerOrderType.Mobile => OrderType.Mobile,
            MessageBrokerOrderType.WebSite => OrderType.Web,
            MessageBrokerOrderType.Api => OrderType.Api,
            _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
        };
    }
}