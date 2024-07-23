using FluentMigrator;
using CertainCourse.OrderService.DataAccess.Postgres.Common;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

namespace CertainCourse.OrderService.DataAccess.Postgres.Migrations;

[Migration(0, "Initial migration")]
public class Initial : ShardSqlMigration
{
    protected override string GetUpSql(IServiceProvider services) =>
        """
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'order_type') THEN
                    CREATE TYPE public.order_type AS ENUM ('Web', 'Mobile', 'Api');
                END IF;
                
                IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'order_state') THEN
                    CREATE TYPE public.order_state AS ENUM ('Created', 'SentToCustomer', 'Delivered', 'Lost', 'Cancelled');
                END IF;
            END$$;
        
            CREATE TABLE IF NOT EXISTS orders (
                id BIGSERIAL PRIMARY KEY,
                goods_count INT NOT NULL,
                total_amount DECIMAL NOT NULL,
                total_weight DOUBLE PRECISION NOT NULL,
                type public.order_type NOT NULL,
                create_date TIMESTAMP NOT NULL,
                region_id INT NOT NULL,
                state public.order_state NOT NULL,
                customer_id INT NOT NULL,
                customer_first_name TEXT NOT NULL,
                customer_last_name TEXT NOT NULL,
                customer_address JSONB NOT NULL,
                customer_mobile_number TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS regions (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                storage_id INT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS storages (
                id SERIAL PRIMARY KEY,
                latitude DOUBLE PRECISION NOT NULL,
                longitude DOUBLE PRECISION NOT NULL
            );

            CREATE TABLE IF NOT EXISTS new_orders_outbox_messages (
                id BIGSERIAL PRIMARY KEY,
                type TEXT NOT NULL,
                payload TEXT NOT NULL,
                created_at TIMESTAMP NOT NULL,
                processed_at TIMESTAMP
            );
        """;

    protected override string GetDownSql(IServiceProvider services) =>
        $"""
             DROP TABLE IF EXISTS new_orders_outbox_messages;
             DROP TABLE IF EXISTS storages;
             DROP TABLE IF EXISTS regions;
             DROP TABLE IF EXISTS orders;
             DROP TYPE order_type;
             DROP TYPE order_state;
         """;
}