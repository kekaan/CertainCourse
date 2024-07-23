using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Sharded;

internal abstract class BaseShardedRepository
{
    private readonly IShardingRule<int> _intShardingRule;
    private readonly IShardingRule<long> _longShardingRule;
    private readonly IShardingRule<string> _stringShardingRule;
    private readonly IShardConnectionFactory _connectionFactory;

    protected BaseShardedRepository(
        IShardingRule<int> intShardingRule,
        IShardConnectionFactory connectionFactory,
        IShardingRule<string> stringShardingRule,
        IShardingRule<long> longShardingRule)
    {
        _intShardingRule = intShardingRule;
        _connectionFactory = connectionFactory;
        _stringShardingRule = stringShardingRule;
        _longShardingRule = longShardingRule;
    }
    
    protected ShardNpgsqlConnection GetConnectionByShardKey(
        long shardKey)
    {
        var bucketId = _longShardingRule.GetBucketId(shardKey);
        return _connectionFactory.GetConnectionByBucket(bucketId);
    }
    
    protected ShardNpgsqlConnection GetConnectionByShardKey(
        int shardKey)
    {
        var bucketId = _intShardingRule.GetBucketId(shardKey);
        return _connectionFactory.GetConnectionByBucket(bucketId);
    }

    protected ShardNpgsqlConnection GetConnectionByShardKey(
        string shardKey)
    {
        var bucketId = _stringShardingRule.GetBucketId(shardKey);
        return _connectionFactory.GetConnectionByBucket(bucketId);
    }

    protected ShardNpgsqlConnection GetConnectionByBucket(
        int bucketId)
    {
        return _connectionFactory.GetConnectionByBucket(bucketId);
    }
    
    protected ShardNpgsqlConnection GetRandomConnection()
    {
        return _connectionFactory.GetRandomBucketConnection();
    }

    protected int GetBucketByShardKey(
        int shardKey) => _intShardingRule.GetBucketId(shardKey);
    
    protected int GetBucketByShardKey(
        long shardKey) => _longShardingRule.GetBucketId(shardKey);

    protected IEnumerable<int> AllBuckets => _connectionFactory.GetAllBuckets();
}