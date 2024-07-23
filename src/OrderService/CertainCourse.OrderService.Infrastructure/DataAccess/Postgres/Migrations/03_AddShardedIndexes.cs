using FluentMigrator;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

namespace CertainCourse.OrderService.DataAccess.Postgres.Migrations;

[Migration(3, "Adding indexes for sharded tables")]
public class AddShardedIndexes : ShardSqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        $"""
             CREATE TABLE IF NOT EXISTS idx_order_customer_id(
                 customer_id INT NOT NULL,
                 order_id BIGSERIAL NOT NULL,
                 PRIMARY KEY (customer_id, order_id));
         """;

    protected override string GetDownSql(IServiceProvider services) =>
        $"""
             DROP TABLE IF EXISTS idx_order_customer_id;
         """;
}