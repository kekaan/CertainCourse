using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Infrastructure.Configuration;
using StackExchange.Redis;

namespace CertainCourse.OrderService.Infrastructure.Redis.Repositories;

public class BaseRedisRepository
{
    private readonly string _keyPrefix;

    protected readonly int _db;
    protected readonly IServer _server;
    protected readonly IDatabaseAsync _database;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    protected RedisKey BuildKey(object id) =>
        new($"{_keyPrefix}:{id}");

    protected static string ToRedisString<TValue>(TValue value) =>
        JsonSerializer.Serialize(value, JsonSerializerOptions);

    protected static TValue? ToObject<TValue>(RedisValue redisValue) =>
        redisValue.HasValue
            ? JsonSerializer.Deserialize<TValue>(redisValue.ToString(), JsonSerializerOptions)
            : default;

    internal BaseRedisRepository(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> redisSettings,
        string keyPrefix)
    {
        _db = redisSettings.Value.Server;
        _database = connectionMultiplexer.GetDatabase(_db);
        _server = connectionMultiplexer.GetServers().First();
        _keyPrefix = keyPrefix;
    }

    protected async Task IsKeyNotExistsOrThrow(RedisKey key)
    {
        var isKeyExist = await _database.KeyExistsAsync(key);

        if (isKeyExist)
        {
            throw new InvalidOperationException($"Key {isKeyExist.ToString()} already exists");
        }
    }

    protected async Task IsKeyExistsOrThrow(RedisKey key)
    {
        var isKeyExist = await _database.KeyExistsAsync(key);

        if (!isKeyExist)
        {
            throw new KeyNotFoundException($"Key {key.ToString()} not found");
        }
    }
}