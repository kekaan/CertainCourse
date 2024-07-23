namespace CertainCourse.OrderService.Application.OrderStateChanged;

public record OrderStateChangedEvent(long OrderId, OrderState OrderState, DateTimeOffset ChangedAt);