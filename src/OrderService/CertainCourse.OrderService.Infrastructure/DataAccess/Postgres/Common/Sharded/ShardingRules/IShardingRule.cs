namespace CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;

internal interface IShardingRule<TShardKey>
{
    int GetBucketId(TShardKey shardKey);
}