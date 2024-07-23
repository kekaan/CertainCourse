using Murmur;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.ShardingRules;

internal sealed class IntShardingRule : IShardingRule<int>
{
    private readonly IDbStore _dbStore;

    public IntShardingRule(
        IDbStore dbStore)
    {
        _dbStore = dbStore;
    }

    public int GetBucketId(
        int shardKey)
    {
        var shardKeyHashCode = GetShardKeyHashCode(shardKey);
        if (_dbStore.BucketsCount == 0)
        {
            throw new Exception("DbStore not ready yet");
        }

        var bucketId = Math.Abs(shardKeyHashCode) % _dbStore.BucketsCount;
        return bucketId;
    }

    private int GetShardKeyHashCode(
        int shardKey)
    {
        var bytes = BitConverter.GetBytes(shardKey);
        var murmur = MurmurHash.Create32();
        var hash = murmur.ComputeHash(bytes);
        return BitConverter.ToInt32(hash);
    }
}