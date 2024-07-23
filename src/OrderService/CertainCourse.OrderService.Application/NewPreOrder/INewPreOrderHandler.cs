using CertainCourse.OrderService.Application.MessageBrokers;

namespace CertainCourse.OrderService.Application.NewPreOrder;

public interface INewPreOrderHandler : IHandler<NewPreOrderEvent>
{
}