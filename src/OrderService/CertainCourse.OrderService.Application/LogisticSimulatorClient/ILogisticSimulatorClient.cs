namespace CertainCourse.OrderService.Application.LogisticSimulatorClient;

public interface ILogisticSimulatorClient
{
    Task<LogisticSimulatorCancelOrderResult> CancelOrderAsync(long orderId, CancellationToken cancellationToken);
}