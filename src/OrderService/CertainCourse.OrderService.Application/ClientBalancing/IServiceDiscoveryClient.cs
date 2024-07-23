namespace CertainCourse.OrderService.Application.ClientBalancing;

public interface IServiceDiscoveryClient
{
    IAsyncEnumerable<ClusterEndpoints> GetEndpoints(string clusterName, CancellationToken token);
}