using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using CertainCourse.OrderService.Application.ClientBalancing;
using CertainCourse.OrderService.DataAccess.Postgres.Migrations;
using CertainCourse.OrderService.Infrastructure.Configuration;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;

public class ShardMigrator
{
    private readonly IServiceDiscoveryClient _serviceDiscoveryClient;
    private readonly ShardDbOptions _dbOptions;

    public ShardMigrator(
        IServiceDiscoveryClient serviceDiscoveryClient,
        IOptions<ShardDbOptions> dbOptions)
    {
        _serviceDiscoveryClient = serviceDiscoveryClient;
        _dbOptions = dbOptions.Value;
    }

    public async Task Migrate(
        CancellationToken token)
    {
        var endpoints = await GetEndpoints(token);

        foreach (var endpoint in endpoints)
        {
            var connectionString = GetConnectionString(endpoint);
            foreach (var bucketId in endpoint.Buckets)
            {
                var serviceProvider = CreateServices(connectionString);
                using var scope = serviceProvider.CreateScope();
                var migrationContext = scope.ServiceProvider.GetRequiredService<BucketMigrationContext>();
                migrationContext.UpdateCurrentDbSchema(bucketId);
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }
    }

    private string GetConnectionString(
        DbEndpoint endpoint)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = endpoint.HostAndPort,
            Database = _dbOptions.DatabaseName,
            Username = _dbOptions.User,
            Password = _dbOptions.Password
        };
        return builder.ToString();
    }

    private IServiceProvider CreateServices(string connectionString)
    {
        return new ServiceCollection()
            .AddSingleton<BucketMigrationContext>()
            .AddFluentMigratorCore()
            .AddLogging(o => o.AddFluentMigratorConsole())
            .ConfigureRunner(
                builder => builder
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .WithMigrationsIn(typeof(Initial).Assembly)
                    .ScanIn(typeof(ShardVersionTableMetaData).Assembly).For.VersionTableMetaData()
            )
            .BuildServiceProvider();
    }

    private async Task<DbEndpoint[]> GetEndpoints(CancellationToken token)
    {
        await using var enumerator = _serviceDiscoveryClient
            .GetEndpoints(_dbOptions.ClusterName, token)
            .GetAsyncEnumerator(token);

        await enumerator.MoveNextAsync();

        return enumerator.Current.Endpoints;
    }
}