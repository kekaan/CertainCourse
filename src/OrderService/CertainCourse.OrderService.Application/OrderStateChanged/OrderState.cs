namespace CertainCourse.OrderService.Application.OrderStateChanged;

public enum OrderState
{
    Created = 1,
    SentToCustomer = 2,
    Delivered = 3,
    Lost = 4,
    Cancelled = 5
}