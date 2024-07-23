using Murmur;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.ShardingRules;

internal sealed class LongShardingRule : IShardingRule<long>
{
    private readonly IDbStore _dbStore;

    public LongShardingRule(
        IDbStore dbStore)
    {
        _dbStore = dbStore;
    }

    public int GetBucketId(
        long shardKey)
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
        long shardKey)
    {
        var bytes = BitConverter.GetBytes(shardKey);
        var murmur = MurmurHash.Create32();
        var hash = murmur.ComputeHash(bytes);
        return BitConverter.ToInt32(hash);
    }
}