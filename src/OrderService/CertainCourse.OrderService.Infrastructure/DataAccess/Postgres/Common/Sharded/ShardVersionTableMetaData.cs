using FluentMigrator.Runner.VersionTableInfo;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;

public class ShardVersionTableMetaData: IVersionTableMetaData
{
    private readonly BucketMigrationContext _migrationContext;

    public ShardVersionTableMetaData(
        BucketMigrationContext migrationContext)
    {
        _migrationContext = migrationContext;
    }

    public object ApplicationContext { get; set; }
    public bool OwnsSchema => true;
    public string SchemaName => _migrationContext.CurrentDbSchema;
    public string TableName => "version_info";
    public string ColumnName => "version";
    public string DescriptionColumnName => "description";
    public string UniqueIndexName => "version_unique_idx";
    public string AppliedOnColumnName => "applied_on";
}