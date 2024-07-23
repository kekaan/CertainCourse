using Grpc.Core;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;

namespace CertainCourse.GatewayService.Infrastructure.Extensions;

internal static class StatusCodeExtension
{
    internal static CallResultStatusCode ToCallResultStatusCode(this StatusCode statusCode)
    {
        return statusCode switch
        {
            StatusCode.OK => CallResultStatusCode.OK,
            StatusCode.Cancelled => CallResultStatusCode.Cancelled,
            StatusCode.Unknown => CallResultStatusCode.Unknown,
            StatusCode.InvalidArgument => CallResultStatusCode.InvalidArgument,
            StatusCode.DeadlineExceeded => CallResultStatusCode.DeadlineExceeded,
            StatusCode.NotFound => CallResultStatusCode.NotFound,
            StatusCode.AlreadyExists => CallResultStatusCode.AlreadyExists,
            StatusCode.PermissionDenied => CallResultStatusCode.PermissionDenied,
            StatusCode.Unauthenticated => CallResultStatusCode.Unauthenticated,
            StatusCode.ResourceExhausted => CallResultStatusCode.ResourceExhausted,
            StatusCode.FailedPrecondition => CallResultStatusCode.FailedPrecondition,
            StatusCode.Aborted => CallResultStatusCode.Aborted,
            StatusCode.OutOfRange => CallResultStatusCode.OutOfRange,
            StatusCode.Unimplemented => CallResultStatusCode.Unimplemented,
            StatusCode.Internal => CallResultStatusCode.Internal,
            StatusCode.Unavailable => CallResultStatusCode.Unavailable,
            StatusCode.DataLoss => CallResultStatusCode.DataLoss,
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, null)
        };
    }
}