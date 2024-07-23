using CertainCourse.OrderService.Application.MessageBrokers;

namespace CertainCourse.OrderService.Application.OrderStateChanged;

public interface IOrderStateChangedHandler : IHandler<OrderStateChangedEvent>
{
}