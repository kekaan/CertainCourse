using FluentMigrator;
using CertainCourse.OrderService.DataAccess.Postgres.Common;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

namespace CertainCourse.OrderService.DataAccess.Postgres.Migrations;

[Migration(2, "Adding indexes migration")]
public class AddIndexes : ShardSqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        """
            CREATE INDEX orders_region_id_idx ON orders (region_id);
            CREATE INDEX orders_customer_id_idx ON orders (customer_id);
            CREATE INDEX orders_create_date_idx ON orders (create_date);
            CREATE INDEX new_orders_outbox_messages_type_idx ON new_orders_outbox_messages (type);
        """;

    protected override string GetDownSql(IServiceProvider services) =>
        $"""
            DROP INDEX orders_region_id_idx;
            DROP INDEX orders_customer_id_idx;
            DROP INDEX orders_create_date_idx;
            DROP INDEX new_orders_outbox_messages_type_idx;
         """;
}