using System.Text;
using System.Text.Json;
using System.Transactions;
using Dapper;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Application.MessageBrokers.Models;
using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.DataAccess.Common.Extensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Base;

internal sealed class OrderDbRepository : BaseDbRepository, IOrderDbRepository
{
    private const string FIELDS =
        "id, goods_count, total_amount, total_weight, type, create_date, region_id, state, customer_id," +
        "customer_first_name, customer_first_name, customer_address, customer_mobile_number";
    private const string TABLE = "orders";

    private const string REGION_TABLE_FIELDS = "id, name, storage_id";
    private const string REGION_TABLE = "regions";

    private const string OUTBOX_TABLE_FIELDS = "id, type, payload, created_at, processed_at";
    private const string OUTBOX_TABLE = $"new_orders_outbox_messages";
    
    public OrderDbRepository(IDatabaseConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<OrderDal> GetByIdAsync(long id, CancellationToken cancellationToken)
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
        var order = await connection.QuerySingleOrDefaultAsync<OrderDal>(command);

        return order ?? throw new KeyNotFoundException($"No order with this id: {id}");
    }

    public async Task<PagedCollection<OrderEntity>> GetAllAsync(CancellationToken cancellationToken,
        PaginationSpecification? paginationSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"""
                                 select {FIELDS}, {REGION_TABLE_FIELDS}
                                 from {TABLE} as o join {REGION_TABLE} as r on o.region_id = o.id;
                             """);

        ApplyFiltering(queryBuilder, filteringSpecifications);
        ApplySorting(queryBuilder, sortingSpecifications);
        ApplyPagination(queryBuilder, paginationSpecification);

        var command = new CommandDefinition(
            queryBuilder.ToString(),
            cancellationToken: cancellationToken);

        using var connection = _connectionFactory.GetConnection();

        var orders = await connection.QueryAsync(command,
            (OrderDal order, RegionDal region) => order.ToDomain(region),
            splitOn: "id");

        return paginationSpecification is not null
            ? orders.ToArray().ToPagedList(paginationSpecification.PageSize, paginationSpecification.PageToken,
            int.Parse(paginationSpecification.PageToken + 1).ToString())
            : orders.ToArray().ToPagedList(0, string.Empty, string.Empty);
    }

    private static void ApplyPagination(StringBuilder queryBuilder, PaginationSpecification? paginationSpecification)
    {
        if (paginationSpecification is null)
            return;

        queryBuilder.Append(
            $" limit {paginationSpecification.PageSize} offset {int.Parse(paginationSpecification.PageToken) * paginationSpecification.PageSize}");
    }

    private void ApplySorting(StringBuilder queryBuilder,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications)
    {
        if (sortingSpecifications is null || sortingSpecifications.Count is 0)
            return;

        var sortingConditions = sortingSpecifications.Select(spec =>
            $"{spec.Property.ToDatabaseColumn()} {(spec.SortDescending ? "desc" : "asc")}");

        queryBuilder.Append(" order by ");
        queryBuilder.Append(string.Join(", ", sortingConditions));
    }

    private void ApplyFiltering(StringBuilder queryBuilder,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications)
    {
        if (filteringSpecifications is null || filteringSpecifications.Count is 0)
            return;

        var filterConditions = new List<string>();
        foreach (var filter in filteringSpecifications)
        {
            if (filter.MinCreateDateTime.HasValue)
            {
                filterConditions.Add($"create_date >= {filter.MinCreateDateTime}");
            }

            if (filter.RegionIdPossibleValues is not null && filter.RegionIdPossibleValues.Any())
            {
                var regionIds = string.Join(",", filter.RegionIdPossibleValues);
                filterConditions.Add($"region_id in ({regionIds})");
            }

            if (filter.CustomerIdPossibleValues is not null && filter.CustomerIdPossibleValues.Any())
            {
                var customerIds = string.Join(",", filter.CustomerIdPossibleValues);
                filterConditions.Add($"customer_id in ({customerIds})");
            }
        }

        if (filterConditions.Count is 0)
            return;

        queryBuilder.Append(" where ");
        queryBuilder.Append(string.Join(" and ", filterConditions));
    }

    public async Task InsertAsync(
        OrderDal order,
        bool shouldBeSentToOutbox,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.GetConnection();
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                IsolationLevel = IsolationLevel.ReadCommitted
            },
            TransactionScopeAsyncFlowOption.Enabled);
        
        var command = PrepareOrderInsertCommand(order, cancellationToken);
        await connection.QueryAsync(command);

        if (shouldBeSentToOutbox)
        {
            var outboxCommand = PrepareOutboxInsertCommand(order, cancellationToken);
            await connection.QueryAsync(outboxCommand);
        }

        transaction.Complete();
    }

    private static CommandDefinition PrepareOrderInsertCommand(OrderDal order, CancellationToken cancellationToken)
    {
        var query = $"""
                         INSERT INTO {TABLE} ({FIELDS})
                         VALUES (@Id, @GoodsCount, @TotalAmount, @TotalWeight, @Type::order_type, @CreateDate,
                                 @RegionId, @State::order_state, @CustomerId, );
                     """;

        DynamicParameters parameters = new();
        parameters.Add("Id", order.Id);
        parameters.Add("GoodsCount", order.GoodsCount);
        parameters.Add("TotalAmount", order.TotalAmount);
        parameters.Add("TotalWeight", order.TotalWeight);
        parameters.Add("Type", order.Type.ToString());
        parameters.Add("CreateDate", order.CreateDate);
        parameters.Add("RegionId", order.RegionId);
        parameters.Add("State", order.State.ToString());
        parameters.Add("CustomerId", order.CustomerId);

        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        return command;
    }
    
    private CommandDefinition PrepareOutboxInsertCommand(OrderDal order, CancellationToken cancellationToken)
    {
        const string outboxQuery = $"""
                                        INSERT INTO {OUTBOX_TABLE} ({OUTBOX_TABLE_FIELDS})
                                        VALUES (@Id, @Type, @Payload, @CreatedAt, @ProcessedAt);
                                    """;

        var outboxMessage = new NewOrdersOutboxMessage
        (
            Id: order.Id,
            Type: nameof(NewOrder),
            Payload: JsonSerializer.Serialize(new NewOrder(order.Id)),
            CreatedAt: DateTime.UtcNow,
            ProcessedAt: null
        );

        DynamicParameters outboxParameters = new();
        outboxParameters.Add("Id", outboxMessage.Id);
        outboxParameters.Add("Type", outboxMessage.Type);
        outboxParameters.Add("Payload", outboxMessage.Payload);
        outboxParameters.Add("CreatedAt", outboxMessage.CreatedAt);
        outboxParameters.Add("ProcessedAt", outboxMessage.ProcessedAt);

        var outboxCommand = new CommandDefinition(
            outboxQuery,
            parameters: outboxParameters,
            cancellationToken: cancellationToken);

        return outboxCommand;
    }
    
    public async Task UpdateAsync(
        OrderDal order,
        CancellationToken cancellationToken)
    {
        const string query =
            $"""
                UPDATE {TABLE}
                SET id = @Id, goods_count = @GoodsCount, total_amount = @TotalAmount, total_weight = @TotalWeight,
                    type = @Type::order_type, create_date = @CreateDate, region_id = @RegionId, state = @State::order_state, customer_id = @CustomerId
                WHERE id = @id
             """;

        DynamicParameters parameters = new();
        parameters.Add("Id", order.Id);
        parameters.Add("GoodsCount", order.GoodsCount);
        parameters.Add("TotalAmount", order.TotalAmount);
        parameters.Add("TotalWeight", order.TotalWeight);
        parameters.Add("Type", order.Type.ToString());
        parameters.Add("CreateDate", order.CreateDate);
        parameters.Add("RegionId", order.RegionId);
        parameters.Add("State", order.State.ToString());
        parameters.Add("CustomerId", order.CustomerId);

        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);

        using var connection = _connectionFactory.GetConnection();
        await connection.ExecuteAsync(command);
    }
}