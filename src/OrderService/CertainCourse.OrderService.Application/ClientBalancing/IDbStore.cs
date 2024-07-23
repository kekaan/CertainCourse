namespace CertainCourse.OrderService.Application.ClientBalancing;

public interface IDbStore
{
    Task UpdateEndpointAsync(IReadOnlyCollection<DbEndpoint> endpoints);
    
    DbEndpoint GetEndpointByBucket(int bucketId);

    int BucketsCount { get; }

    Task WaitWarmup();
}