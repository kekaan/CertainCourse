namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

public class BucketMigrationContext
{
    private string _currentDbSchema;

    public void UpdateCurrentDbSchema(
        int bucketId) => _currentDbSchema = $"bucket_{bucketId}";

    public string CurrentDbSchema => _currentDbSchema ??
        throw new InvalidOperationException("Current db schema hasn't been set");
}