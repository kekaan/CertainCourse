namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;

internal static class BucketHelper
{
    public const string BucketPlaceholder = "__bucket__";
    
    public static string GetSchemaName(int bucketId) => $"bucket_{bucketId}";
}