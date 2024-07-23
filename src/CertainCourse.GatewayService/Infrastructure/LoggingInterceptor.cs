using Grpc.Core;
using Grpc.Core.Interceptors;

namespace CertainCourse.GatewayService.Infrastructure;

public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var result = base.AsyncUnaryCall(request, context, continuation);

        _logger.LogInformation(
            $"Called {context.Method.Name} for service {context.Method.ServiceName}");

        return result;
    }
}