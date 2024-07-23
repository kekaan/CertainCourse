using CertainCourse.LogisticsSimulator.Grpc;
using CertainCourse.OrderService.Application.LogisticSimulatorClient;
using LogisticsSimulatorGrpcClient =
    CertainCourse.LogisticsSimulator.Grpc.LogisticsSimulatorService.LogisticsSimulatorServiceClient;

namespace CertainCourse.OrderService.Infrastructure.LogisticSimulatorClient;

public class LogisticSimulatorClient : ILogisticSimulatorClient
{
    private readonly LogisticsSimulatorGrpcClient _logisticsSimulatorGrpcClient;

    public LogisticSimulatorClient(LogisticsSimulatorGrpcClient logisticsSimulatorGrpcClient)
    {
        _logisticsSimulatorGrpcClient = logisticsSimulatorGrpcClient;
    }

    public async Task<LogisticSimulatorCancelOrderResult> CancelOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        Order orderRequest = new()
        {
            Id = orderId
        };

        var response =
            await _logisticsSimulatorGrpcClient.OrderCancelAsync(orderRequest, cancellationToken: cancellationToken);

        return new LogisticSimulatorCancelOrderResult(response.Success, response.Error);
    }
}