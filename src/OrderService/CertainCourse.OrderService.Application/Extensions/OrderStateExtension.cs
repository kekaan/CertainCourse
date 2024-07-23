using CertainCourse.OrderService.Domain.ValueObjects.Order;
using MessageBrokerOrderState = CertainCourse.OrderService.Application.OrderStateChanged.OrderState;

namespace CertainCourse.OrderService.Application.Extensions;

internal static class OrderStateExtension
{
    public static OrderState ToDomain(this MessageBrokerOrderState orderState)
    {
        return orderState switch
        {
            MessageBrokerOrderState.Created => OrderState.Created,
            MessageBrokerOrderState.SentToCustomer => OrderState.SentToCustomer,
            MessageBrokerOrderState.Delivered => OrderState.Delivered,
            MessageBrokerOrderState.Lost => OrderState.Lost,
            MessageBrokerOrderState.Cancelled => OrderState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderState), orderState, null)
        };
    }
}