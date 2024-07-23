using CertainCourse.OrderService.Application.MessageBrokers;

namespace CertainCourse.OrderService.Application.OrderStateChanged;

public class OrderStateChangedException : HandlerException
{
    public OrderStateChangedException(string message): base(message)
    {
    }
}