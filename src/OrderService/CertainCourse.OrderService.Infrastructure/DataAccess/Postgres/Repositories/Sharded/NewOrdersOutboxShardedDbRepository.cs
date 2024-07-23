using System.Text;
using Dapper;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Interfaces;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Sharded;

internal sealed class NewOrdersOutboxShardedDbRepository : BaseShardedRepository, INewOrdersOutboxDbRepository
{
    private const string FIELDS = "id, type, payload, created_at, processed_at";
    private const string TABLE = $"{BucketHelper.BucketPlaceholder}.new_orders_outbox_messages";

    public NewOrdersOutboxShardedDbRepository(IShardingRule<int> intShardingRule,
        IShardConnectionFactory connectionFactory,
        IShardingRule<string> stringShardingRule,
        IShardingRule<long> longShardingRule) : base(
        intShardingRule,
        connectionFactory,
        stringShardingRule,
        longShardingRule)
    {
    }

    public async Task InsertAsync(NewOrdersOutboxDal newOrdersOutboxMessage, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  INSERT INTO {TABLE} ({FIELDS})
                                  VALUES (@Id, @Type, @Payload, @CreatedAt, @ProcessedAt);
                              """;

        DynamicParameters parameters = new();
        parameters.Add("Id", newOrdersOutboxMessage.Id);
        parameters.Add("Type", newOrdersOutboxMessage.Type);
        parameters.Add("Payload", newOrdersOutboxMessage.Payload);
        parameters.Add("CreatedAt", newOrdersOutboxMessage.CreatedAt);
        parameters.Add("ProcessedAt", newOrdersOutboxMessage.ProcessedAt);

        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);

        await using var connection = GetConnectionByShardKey(newOrdersOutboxMessage.Id);
        await connection.QueryAsync(command);
    }

    public async Task<IReadOnlyCollection<NewOrdersOutboxDal>> GetUnprocessedMessagesAsync(
        CancellationToken cancellationToken,
        string? type = null)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"""
                                 SELECT {FIELDS}
                                 FROM {TABLE}
                                 WHERE PROCESSED_AT IS NULL
                             """);

        if (!string.IsNullOrEmpty(type))
        {
            queryBuilder.Append($" and type = '{type}'");
        }

        var result = new List<NewOrdersOutboxDal>();

        foreach (var bucketId in AllBuckets)
        {
            var command = new CommandDefinition(
                queryBuilder.ToString(),
                cancellationToken: cancellationToken);

            await using var connection = GetConnectionByBucket(bucketId);
            var messages = await connection.QueryAsync<NewOrdersOutboxDal>(command);
            result.AddRange(messages);
        }

        return result;
    }

    public async Task MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  UPDATE {TABLE} 
                                  SET processed_at = @datetime
                                  WHERE id = @messageId
                              """;

        DynamicParameters parameters = new();
        parameters.Add("datetime", DateTime.UtcNow);
        parameters.Add("messageId", messageId);

        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);

        await using var connection = GetConnectionByShardKey(messageId);
        await connection.QueryAsync(command);
    }
}