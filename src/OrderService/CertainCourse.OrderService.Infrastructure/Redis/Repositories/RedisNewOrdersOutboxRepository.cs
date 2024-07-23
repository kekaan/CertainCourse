using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.Redis.Repositories;
using StackExchange.Redis;

namespace CertainCourse.OrderService.DataAccess.Repositories.RedisRepositories;

internal sealed class RedisNewOrdersOutboxRepository : BaseRedisRepository, INewOrdersOutboxRepository
{
    private const string OUTBOX_KEY_PREFIX = "outbox";

    public RedisNewOrdersOutboxRepository(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> redisSettings) :
        base(connectionMultiplexer, redisSettings, OUTBOX_KEY_PREFIX)
    {
    }

    public async Task InsertAsync(NewOrdersOutboxMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(message.Id);

        await IsKeyNotExistsOrThrow(key);

        var resultRedis = ToRedisString(message);

        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task<IReadOnlyCollection<NewOrdersOutboxMessage>> GetUnprocessedMessagesAsync(
        CancellationToken cancellationToken, string? type = null)
    {
        var messages = await GetOutboxMessagesAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return type is not null
            ? messages.Where(m => m.Type == type).ToArray()
            : messages.ToArray();
    }

    public async Task MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken)
    {
        var message = await GetMessageByIdAsync(messageId, cancellationToken);

        var newMessage = message with { ProcessedAt = DateTime.UtcNow };
        var resultRedis = ToRedisString(newMessage);
        var key = BuildKey(messageId);

        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task<NewOrdersOutboxMessage> GetMessageByIdAsync(long messageId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(messageId);

        await IsKeyExistsOrThrow(key);

        var resultRedis = await _database.StringGetAsync(key);

        var result = ToObject<NewOrdersOutboxMessage>(resultRedis);

        return result;
    }

    private async Task<IReadOnlyCollection<NewOrdersOutboxMessage>> GetOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keys = _server.KeysAsync(database: _db, pattern: $"{OUTBOX_KEY_PREFIX}*");
        var messagesFromRedis = new ConcurrentBag<NewOrdersOutboxMessage>();

        await foreach (var redisKey in keys)
        {
            var messageRedis = await _database.StringGetAsync(redisKey);
            var message = ToObject<NewOrdersOutboxMessage>(messageRedis.ToString());

            messagesFromRedis.Add(message);
        }

        return messagesFromRedis.ToArray();
    }
}