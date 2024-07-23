using CertainCourse.OrderService.Application.MessageBrokers;

namespace CertainCourse.OrderService.Application.NewPreOrder;

public class NewPreOrderException : HandlerException
{
    public NewPreOrderException(string message): base(message)
    {
    }
}