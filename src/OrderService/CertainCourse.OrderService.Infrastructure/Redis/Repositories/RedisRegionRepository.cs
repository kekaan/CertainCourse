using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.Redis.Repositories;
using StackExchange.Redis;

namespace CertainCourse.OrderService.DataAccess.Repositories.RedisRepositories;

internal sealed class RedisRegionRepository : BaseRedisRepository, IRegionRepository
{
    private const string REGION_KEY_PREFIX = "regions";
    
    public RedisRegionRepository(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> redisSettings) :
        base(connectionMultiplexer, redisSettings, REGION_KEY_PREFIX)
    {
    }
    
    public async Task<IReadOnlyCollection<RegionEntity>> GetRegionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var keys = _server.KeysAsync(database: _db, pattern: $"{REGION_KEY_PREFIX}*");
        var regionsFromRedis = new ConcurrentBag<RegionEntity>();
        
        await foreach (var redisKey in keys)
        {
            var regionRedis = await _database.StringGetAsync(redisKey);
            var region = ToObject<RegionEntity>(regionRedis.ToString());

            regionsFromRedis.Add(region);
        }

        return regionsFromRedis.ToArray();
    }

    public async Task CreateRegionAsync(RegionEntity region, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(region.Id);

        await IsKeyNotExistsOrThrow(key);

        var resultRedis = ToRedisString(region);

        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task<RegionEntity> FindRegionByNameAsync(string regionName, CancellationToken cancellationToken)
    {
        var regions = await GetRegionsAsync(cancellationToken);

        return regions.Single(e => e.Name == regionName);
    }

    public Task<bool> IsRegionExistAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var key = BuildKey(id);
        
        return _database.KeyExistsAsync(key);
    }
}