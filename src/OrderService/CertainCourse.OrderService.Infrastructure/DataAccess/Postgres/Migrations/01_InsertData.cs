using FluentMigrator;
using CertainCourse.OrderService.DataAccess.Postgres.Common;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

namespace CertainCourse.OrderService.DataAccess.Postgres.Migrations;

[Migration(1, "Insert regions data migration")]
public class InsertData : ShardSqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        """
            insert into storages (id, latitude, longitude)
            values (1, 55.700608, 37.588771);

            insert into storages (id, latitude, longitude)
            values (2, 59.910008, 30.351155);

            insert into storages (id, latitude, longitude)
            values (3, 54.973413, 82.856013);

            insert into regions (id, name, storage_id)
            values (1, 'Moscow', 1);

            insert into regions (id, name, storage_id)
            values (2, 'StPetersburg', 2);

            insert into regions (id, name, storage_id)
            values (3, 'Novosibirsk', 3);
        """;

    protected override string GetDownSql(IServiceProvider services) =>
        $"""
            delete from regions where id in (1, 2, 3);
            delete from storages where id in (1, 2, 3);
         """;
}