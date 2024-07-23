using Dapper;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;

internal sealed class RegionDbRepository : BaseDbRepository, IRegionDbRepository
{
    private const string FIELDS = "id, name, storage_id";
    private const string TABLE = "regions";

    public RegionDbRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<RegionDal> GetByIdAsync(long id, CancellationToken cancellationToken)
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
        var region = await connection.QuerySingleOrDefaultAsync<RegionDal>(command);
        
        return region ?? throw new KeyNotFoundException($"No region with this id: {id}");
    }

    public async Task<IReadOnlyDictionary<int, RegionDal>> GetByIdsAsync(IReadOnlyCollection<int> regionIds,
        CancellationToken cancellationToken)
    {
        const string query = $"""
                                  select {FIELDS}
                                  from {TABLE}
                                  where id = any(@regionIds);
                              """;

        DynamicParameters parameters = new();
        parameters.Add("regionIds", regionIds);
        
        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        
        using var connection = _connectionFactory.GetConnection();
        var regions = await connection.QueryAsync<RegionDal>(command);

        return regions.ToDictionary(r => r.Id, r => r);
    }

    public async Task<IReadOnlyCollection<RegionDal>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string query = $"""
                                  select {FIELDS}
                                  from {TABLE};
                              """;

        var command = new CommandDefinition(
            query,
            cancellationToken: cancellationToken);
        
        using var connection = _connectionFactory.GetConnection();
        var regions = await connection.QueryAsync<RegionDal>(command);
        return regions.ToArray();
    }

    public async Task<RegionDal> FindByNameAsync(string regionName, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  select {FIELDS}
                                  from {TABLE}
                                  where name = @regionName;
                              """;

        DynamicParameters parameters = new();
        parameters.Add("regionName", regionName);
        
        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        
        using var connection = _connectionFactory.GetConnection();
        var region = await connection.QuerySingleOrDefaultAsync<RegionDal>(command);
        
        return region ?? throw new KeyNotFoundException($"No region with this name: {regionName}");
    }

    public async Task<bool> IsRegionExistAsync(int id, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  SELECT EXISTS(
                                    SELECT 1 FROM {TABLE} 
                                             WHERE id=@id)
                              """;

        DynamicParameters parameters = new();
        parameters.Add("id", id);
        
        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        
        using var connection = _connectionFactory.GetConnection();
        return await connection.QuerySingleAsync<bool>(command);
    }
}