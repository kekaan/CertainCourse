using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.OrderService.BackgroundJobs;
using CertainCourse.OrderService.Infrastructure.Configuration;
using Xunit;

namespace CertainCourse.OrderService.Tests;

public class ServiceDiscoveryConsumerHostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldUpdateEndpoints_WhenDataIsReceived()
    {
        // Arrange
        var mockClient = new Mock<IServiceDiscoveryClient>();
        var mockDbStore = new Mock<IDbStore>();
        var mockLogger = new NullLogger<ServiceDiscoveryConsumerHostedService>();

        var clusterEndpoints = new ClusterEndpoints[]
        {
            new(
                DateTime.Today,
                [new DbEndpoint("127.0.0.1", DbReplicaType.Master, [1, 2, 3])]
            )
        };

        mockClient.Setup(x => x.GetEndpoints(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(clusterEndpoints.ToAsyncEnumerable());

        var service = new ServiceDiscoveryConsumerHostedService(mockClient.Object, mockDbStore.Object, mockLogger,
            Options.Create(new ShardDbOptions { ClusterName = "cluster" }));

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(2000);

        // Act
        await service.StartAsync(cancellationTokenSource.Token);
        await service.StopAsync(CancellationToken.None);

        // Assert
        mockDbStore.Verify(x => x.UpdateEndpointAsync(It.IsAny<IReadOnlyCollection<DbEndpoint>>()), Times.AtLeastOnce);
    }
}