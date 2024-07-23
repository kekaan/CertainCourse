namespace CertainCourse.OrderService.Application.MessageBrokers;

public interface IHandler<in TRequest>
{
    Task<HandlerResult> Handle(TRequest preOrderRequest, CancellationToken cancellationToken);
}