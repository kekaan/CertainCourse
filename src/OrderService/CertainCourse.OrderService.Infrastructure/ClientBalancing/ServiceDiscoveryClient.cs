using Grpc.Core;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.Sd.Grpc;

namespace CertainCourse.OrderService.Infrastructure.ClientBalancing;

public sealed class ServiceDiscoveryClient : IServiceDiscoveryClient
{
    private readonly SdService.SdServiceClient _client;

    public ServiceDiscoveryClient(SdService.SdServiceClient client)
    {
        _client = client;
    }

    public IAsyncEnumerable<ClusterEndpoints> GetEndpoints(string clusterName, CancellationToken token)
    {
        var request = new DbResourcesRequest
        {
            ClusterName = clusterName
        };

        var stream = _client.DbResources(request, cancellationToken: token);

        return stream.ResponseStream
            .ReadAllAsync(token)
            .Select(Map);
    }

    private static ClusterEndpoints Map(DbResourcesResponse response)
    {
        return new ClusterEndpoints(response.LastUpdated.ToDateTime(), response.Replicas
            .Select(r => new DbEndpoint(
                $"{r.Host}:{r.Port}",
                ToDbReplica(r.Type),
                r.Buckets.ToArray()))
            .ToArray());
    }

    private static DbReplicaType ToDbReplica(Replica.Types.ReplicaType replicaType)
    {
        return replicaType switch
        {
            Replica.Types.ReplicaType.Master => DbReplicaType.Master,
            Replica.Types.ReplicaType.Sync => DbReplicaType.Sync,
            Replica.Types.ReplicaType.Async => DbReplicaType.Async,
            _ => throw new ArgumentOutOfRangeException(nameof(replicaType), replicaType, null)
        };
    }
}