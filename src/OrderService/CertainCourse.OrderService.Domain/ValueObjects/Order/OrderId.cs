namespace CertainCourse.OrderService.Domain.ValueObjects.Order;

public readonly record struct OrderId(long Value)
{
    public static implicit operator long(OrderId orderId) => orderId.Value;
}