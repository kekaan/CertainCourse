using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.ShardingRules;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.TypeHandlers;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Assembly assembly)
    {
        const string CONNECTION_STRING_KEY = "OrderServiceDatabase";
        
        var connectionString = configuration.GetConnectionString(CONNECTION_STRING_KEY);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"Connection string is not set");
        }
        
        serviceCollection.AddScoped<IDatabaseConnectionFactory, PostgresConnectionFactory>();
        AddDapperSettings();
        serviceCollection.AddFluentMigrator(connectionString, assembly);
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddShardedDatabase(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IShardConnectionFactory, ShardConnectionFactory>();
        serviceCollection.AddScoped<IShardingRule<int>, IntShardingRule>();
        serviceCollection.AddScoped<IShardingRule<string>, StringShardingRule>();
        serviceCollection.AddScoped<IShardingRule<long>, LongShardingRule>();
        serviceCollection.AddSingleton<ShardMigrator>();

        AddDapperSettings();
        
        return serviceCollection;
    }
    
    private static void AddDapperSettings()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        Dapper.SqlMapper.AddTypeHandler(new JsonTypeHandler<Address>());
    }

    private static IServiceCollection AddFluentMigrator(
        this IServiceCollection serviceCollection,
        string connectionString,
        Assembly assembly)
    {
        serviceCollection.AddFluentMigratorCore()
            .ConfigureRunner(
                builder => builder
                    .AddPostgres()
                    .ScanIn(assembly)
                    .For.Migrations())
            .AddOptions<ProcessorOptions>()
            .Configure(
                options =>
                {
                    options.ConnectionString = connectionString;
                    options.Timeout          = TimeSpan.FromMinutes(1);
                    options.ProviderSwitches = "Force Quote=false";
                });

        return serviceCollection;
    }
}