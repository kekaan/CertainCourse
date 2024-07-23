using CertainCourse.OrderService.Application.ClientBalancing;

namespace CertainCourse.OrderService.Infrastructure.ClientBalancing;

public class DbStore : IDbStore
{
    private const int START_INDEX = 0;
    private readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private DbEndpoint[] _endpoints = Array.Empty<DbEndpoint>();

    public Task UpdateEndpointAsync(IReadOnlyCollection<DbEndpoint> dbEndpoints)
    {
        _endpoints = new DbEndpoint[dbEndpoints.Count];
        var bucketsCount = 0;

        foreach (var (dbEndpoint, index) in dbEndpoints.Zip(Enumerable.Range(START_INDEX, dbEndpoints.Count)))
        {
            _endpoints[index] =  dbEndpoint;
            bucketsCount      += dbEndpoint.Buckets.Length;
        }

        if (BucketsCount != 0 && BucketsCount != bucketsCount)
        {
            throw new InvalidOperationException("Buckets count has been suddenly changed");
        }

        _bucketsCount = bucketsCount;

        if (_tcs.Task.Status is not TaskStatus.RanToCompletion)
        {
            _tcs.SetResult();
        }
        
        return Task.CompletedTask;
    }

    public DbEndpoint GetEndpointByBucket(
        int bucketId)
    {
        var result = _endpoints.FirstOrDefault(x => x.Buckets.Contains(bucketId));
        if (result is null)
        {
            throw new ArgumentOutOfRangeException($"There is no endpoint for bucket {bucketId}");
        }

        return result;
    }

    private int _bucketsCount = default;
    public int BucketsCount => _bucketsCount;
    public Task WaitWarmup() => _tcs.Task;
}