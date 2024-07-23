namespace CertainCourse.OrderService.Domain.ValueObjects.Order;

public enum OrderState
{
    Created = 1,
    SentToCustomer = 2,
    Delivered = 3,
    Lost = 4,
    Cancelled = 5,
}