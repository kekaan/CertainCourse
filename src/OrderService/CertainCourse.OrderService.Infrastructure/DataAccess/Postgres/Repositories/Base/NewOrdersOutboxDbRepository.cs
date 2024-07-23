using System.Text;
using Dapper;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Interfaces;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;

internal sealed class NewOrdersOutboxDbRepository : BaseDbRepository, INewOrdersOutboxDbRepository
{
    private const string FIELDS = "id, type, payload, created_at, processed_at";
    private const string TABLE = "outbox_messages";

    public NewOrdersOutboxDbRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory)
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

        using var connection = _connectionFactory.GetConnection();
        await connection.QueryAsync(command);
    }

    public async Task<IReadOnlyCollection<NewOrdersOutboxDal>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken,
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

        var command = new CommandDefinition(
            queryBuilder.ToString(),
            cancellationToken: cancellationToken);

        using var connection = _connectionFactory.GetConnection();
        var messages = await connection.QueryAsync<NewOrdersOutboxDal>(command);
        return messages.ToArray();
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

        using var connection = _connectionFactory.GetConnection();
        await connection.QueryAsync(command);
    }
}