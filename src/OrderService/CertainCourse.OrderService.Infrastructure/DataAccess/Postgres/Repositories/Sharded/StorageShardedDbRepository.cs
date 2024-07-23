using Dapper;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Sharded;

internal sealed class StorageShardedDbRepository : BaseShardedRepository, IStorageDbRepository
{
    private const string FIELDS = "id, latitude, longitude";
    private const string TABLE = $"{BucketHelper.BucketPlaceholder}.storages";

    public StorageShardedDbRepository(
        IShardingRule<int> intShardingRule,
        IShardConnectionFactory connectionFactory,
        IShardingRule<string> stringShardingRule,
        IShardingRule<long> longShardingRule) : base(
        intShardingRule,
        connectionFactory,
        stringShardingRule,
        longShardingRule)
    {
    }

    public async Task<StorageDal> GetById(int id, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  SELECT {FIELDS}
                                  FROM {TABLE}
                                  WHERE ID = @id;
                              """;

        var command = new CommandDefinition(
            query,
            new { id },
            cancellationToken: cancellationToken);

        await using var connection = GetRandomConnection();
        return await connection.QuerySingleAsync<StorageDal>(command);
    }

    public async Task<IReadOnlyDictionary<int, StorageDal>> GetByIds(
        IReadOnlyCollection<int> storageIds,
        CancellationToken cancellationToken)
    {
        const string query = $"""
                                  SELECT {FIELDS}
                                  FROM {TABLE}
                                  WHERE ID = ANY(@storageIds);
                              """;

        var command = new CommandDefinition(
            query,
            new { storageIds },
            cancellationToken: cancellationToken);

        await using var connection = GetRandomConnection();
        var storages = await connection.QueryAsync<StorageDal>(command);

        return storages.ToDictionary(r => r.Id, r => r);
    }
}