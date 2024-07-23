using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

public abstract class ShardSqlMigration: IMigration
{
    public void GetUpExpressions(IMigrationContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var schema = context.ServiceProvider.GetRequiredService<BucketMigrationContext>().CurrentDbSchema;
        if (!context.QuerySchema.SchemaExists(schema))
        {
            context.Expressions.Add(new ExecuteSqlStatementExpression {SqlStatement = $"create schema {schema};"});
        }

        context.Expressions.Add(new ExecuteSqlStatementExpression { SqlStatement = $"set search_path to {schema};" });
        context.Expressions.Add(new ExecuteSqlStatementExpression { SqlStatement = GetUpSql(context.ServiceProvider) });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var schema = context.ServiceProvider.GetRequiredService<BucketMigrationContext>().CurrentDbSchema;
        if (!context.QuerySchema.SchemaExists(schema))
        {
            context.Expressions.Add(new ExecuteSqlStatementExpression {SqlStatement = $"create schema {schema};"});
        }

        context.Expressions.Add(new ExecuteSqlStatementExpression { SqlStatement = $"set search_path to {schema};" });
        context.Expressions.Add(new ExecuteSqlStatementExpression { SqlStatement = GetDownSql(context.ServiceProvider) });
    }

    protected abstract string GetUpSql(IServiceProvider services);
    protected abstract string GetDownSql(IServiceProvider services);

    object IMigration.ApplicationContext => throw new NotSupportedException();
    string IMigration.ConnectionString => throw new NotSupportedException();
}