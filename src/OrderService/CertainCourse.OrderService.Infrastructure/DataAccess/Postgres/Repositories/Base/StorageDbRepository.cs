using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using Dapper;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Base;

internal sealed class StorageDbRepository : BaseDbRepository, IStorageDbRepository
{
    private const string FIELDS = "id, latitude, longitude";
    private const string TABLE = "storages";

    public StorageDbRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<StorageDal> GetById(int id, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  select {FIELDS}
                                  from {TABLE}
                                  where id = @id;
                              """;
        
        DynamicParameters parameters = new();
        parameters.Add("id", id);
        
        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        
        using var connection = _connectionFactory.GetConnection();
        return await connection.QuerySingleAsync<StorageDal>(command);
    }

    public async Task<IReadOnlyDictionary<int, StorageDal>> GetByIds(IReadOnlyCollection<int> storageIds,
        CancellationToken cancellationToken)
    {
        const string query = $"""
                                  select {FIELDS}
                                  from {TABLE}
                                  where id = any(@storageIds);
                              """;
        
        DynamicParameters parameters = new();
        parameters.Add("storageIds", storageIds);
        
        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        
        using var connection = _connectionFactory.GetConnection();
        var storages = await connection.QueryAsync<StorageDal>(command);

        return storages.ToDictionary(r => r.Id, r => r);
    }
}