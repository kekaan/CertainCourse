using Microsoft.Extensions.Options;
using Npgsql;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.Configuration;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;

internal interface IShardConnectionFactory
{
    ShardNpgsqlConnection GetConnectionByBucket(int bucketId);
    ShardNpgsqlConnection GetRandomBucketConnection();

    IEnumerable<int> GetAllBuckets();
}

internal sealed class ShardConnectionFactory : IShardConnectionFactory
{
    private readonly IDbStore _dbStore;
    private readonly ShardDbOptions _dbOptions;

    public ShardConnectionFactory(
        IDbStore dbStore,
        IOptions<ShardDbOptions> dbOptions)
    {
        _dbStore = dbStore;
        _dbOptions = dbOptions.Value;
    }

    public ShardNpgsqlConnection GetConnectionByBucket(
        int bucketId)
    {
        var endpoint = _dbStore.GetEndpointByBucket(bucketId);
        var connectionString = GetConnectionString(endpoint);
        return new ShardNpgsqlConnection(new NpgsqlConnection(connectionString), bucketId);
    }

    public ShardNpgsqlConnection GetRandomBucketConnection()
    {
        Random random = new Random();
        var bucketId = random.Next(_dbStore.BucketsCount);
        
        return GetConnectionByBucket(bucketId);
    }

    private string GetConnectionString(DbEndpoint endpoint)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = endpoint.HostAndPort,
            Database = _dbOptions.DatabaseName,
            Username = _dbOptions.User,
            Password = _dbOptions.Password
        };
        
        return builder.ToString();
    }

    public IEnumerable<int> GetAllBuckets()
    {
        for (int bucketId = 0; bucketId < _dbStore.BucketsCount; bucketId++)
        {
            yield return bucketId;
        }
    }
}