using Grpc.Core;
using CertainCourse.GatewayService.Infrastructure.Extensions;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;

namespace CertainCourse.GatewayService.Tests.InfrastructureTests;

public class StatusCodeExtensionTests
{
    [Theory]
    [InlineData(StatusCode.OK, CallResultStatusCode.OK)]
    [InlineData(StatusCode.Cancelled, CallResultStatusCode.Cancelled)]
    [InlineData(StatusCode.Unknown, CallResultStatusCode.Unknown)]
    public void ToCallResultStatusCode_ShouldConvertCorrectly(StatusCode grpcStatusCode, CallResultStatusCode expectedStatusCode)
    {
        // Act
        CallResultStatusCode result = grpcStatusCode.ToCallResultStatusCode();

        // Assert
        Assert.Equal(expectedStatusCode, result);
    }
}