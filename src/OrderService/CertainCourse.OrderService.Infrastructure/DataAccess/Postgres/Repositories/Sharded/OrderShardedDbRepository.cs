using System.Text;
using System.Text.Json;
using System.Transactions;
using Dapper;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Application.MessageBrokers.Models;
using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Sharded.ShardingRules;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Sharded;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.DataAccess.Common.Extensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Sharded;

internal sealed class OrderShardedDbRepository : BaseShardedRepository, IOrderDbRepository
{
    private const string FIELDS =
        "id, goods_count, total_amount, total_weight, type, create_date, region_id, state, customer_id, " +
        "customer_first_name, customer_last_name, customer_address, customer_mobile_number";
    private const string TABLE = $"{BucketHelper.BucketPlaceholder}.orders";

    private const string CUSTOMER_INDEX_TABLE = $"{BucketHelper.BucketPlaceholder}.idx_order_customer_id";
    private const string CUSTOMER_ID_INDEX_FIELDS = "customer_id, order_id";
    
    private const string OUTBOX_TABLE_FIELDS = "id, type, payload, created_at, processed_at";
    private const string OUTBOX_TABLE = $"{BucketHelper.BucketPlaceholder}.new_orders_outbox_messages";

    public OrderShardedDbRepository(
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

    public async Task<OrderDal> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string query = $"""
                                  select {FIELDS}
                                  from {TABLE}
                                  where id = @id;
                              """;

        var command = new CommandDefinition(
            query,
            new { id },
            cancellationToken: cancellationToken);

        await using var connection = GetConnectionByShardKey(id);
        var order = await connection.QuerySingleOrDefaultAsync<OrderDal>(command);
        return order ?? throw new KeyNotFoundException($"No order with this id: {id}");
    }

    public async Task<PagedCollection<OrderEntity>> GetAllAsync(CancellationToken cancellationToken,
        PaginationSpecification? paginationSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        var rawData = new List<(int BucketId, OrderEntity Order)>();

        var bucketsToCheck = await GetBucketsAndOrdersToCheckByIndexes(cancellationToken, filteringSpecifications);
        var commonFilteringConditions = ConfigureCommonFilteringConditions(filteringSpecifications);
        var bucketsOffsets = GetBucketsOffsets(paginationSpecification);

        foreach (var (bucketId, idsInBucket) in bucketsToCheck)
        {
            var queryBuilder = BuildBucketQuery(
                commonFilteringConditions,
                idsInBucket,
                bucketsOffsets[bucketId],
                paginationSpecification,
                sortingSpecifications);

            await using var connection = GetConnectionByBucket(bucketId);

            var command = new CommandDefinition(queryBuilder.ToString(), cancellationToken: cancellationToken);

            var ordersInBucket = await connection.QueryAsync(command,
                (OrderDal order, RegionDal region) => (bucketId, order.ToDomain(region)),
                splitOn: "name");

            rawData.AddRange(ordersInBucket);
        }

        var postprocessedData = ApplySpecifications(rawData, paginationSpecification, sortingSpecifications);

        var nextPageToken = GetNextPageToken(postprocessedData, bucketsOffsets);
        var result = postprocessedData
            .Select(e => e.Order)
            .ToArray();

        return paginationSpecification is not null
            ? result.ToPagedList(paginationSpecification.PageSize, paginationSpecification.PageToken, nextPageToken)
            : result.ToPagedList(0, string.Empty, string.Empty);
    }

    private IReadOnlyDictionary<int, int> GetBucketsOffsets(PaginationSpecification? paginationSpecification)
    {
        // Если не заданы, то offset начинается с нуля
        if (paginationSpecification is null || string.IsNullOrEmpty(paginationSpecification.PageToken))
        {
            return AllBuckets.ToDictionary(e => e, _ => 0);
        }

        // Получение оффсетов из строки token'a
        var base64EncodedBytes = Convert.FromBase64String(paginationSpecification.PageToken);
        var bucketsOffsets = new int[base64EncodedBytes.Length / sizeof(int)];
        Buffer.BlockCopy(base64EncodedBytes, 0, bucketsOffsets, 0, base64EncodedBytes.Length);

        // Формирирование словаря
        var result = new Dictionary<int, int>(bucketsOffsets.Length);
        for (var i = 0; i < bucketsOffsets.Length; i++)
        {
            result.Add(i, bucketsOffsets[i]);
        }

        return result;
    }
    
    private StringBuilder BuildBucketQuery(
        List<string> commonFilteringConditions,
        long[] idsInBucket,
        int bucketOffset,
        PaginationSpecification? paginationSpecification,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append(
            $"""
                 select 
                 o.id, o.goods_count, o.total_amount, o.total_weight, o.type, o.create_date, o.region_id, o.state, o.customer_id,
                    o.customer_first_name, o.customer_last_name, o.customer_address, o.customer_mobile_number, r.name, r.storage_id
                 from {BucketHelper.BucketPlaceholder}.orders as o 
                 join {BucketHelper.BucketPlaceholder}.regions as r on o.region_id = r.id
             """);

        ApplyFiltering(queryBuilder, commonFilteringConditions, idsInBucket);
        ApplySorting(queryBuilder, sortingSpecifications);
        ApplyPagination(queryBuilder, paginationSpecification, bucketOffset);

        return queryBuilder;
    }
    
    private void ApplyPagination(StringBuilder queryBuilder, PaginationSpecification? paginationSpecification,
        int bucketOffset)
    {
        if (paginationSpecification is not null)
        {
            queryBuilder.Append($" limit {paginationSpecification.PageSize} offset {bucketOffset}");
        }
    }

    private void ApplyFiltering(StringBuilder queryBuilder, List<string> commonFilteringConditions, long[] filteringIds)
    {
        if (commonFilteringConditions.Count is 0 && filteringIds.Length is 0)
            return;

        var filteringString = new StringBuilder();
        filteringString.Append(" where ");

        if (commonFilteringConditions.Count > 0)
        {
            filteringString.Append(string.Join(" and ", commonFilteringConditions));
        }

        if (filteringIds.Length > 0)
        {
            var filteringIdsStrings = string.Join(",", filteringIds);
            var filteringIdsString = commonFilteringConditions.Count > 0
                ? $" and o.id in ({filteringIdsStrings})"
                : $" id o.in ({filteringIdsStrings})";

            filteringString.Append(filteringIdsString);
        }

        queryBuilder.Append(filteringString);
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
    
    private IEnumerable<(int BucketId, OrderEntity Order)> ApplySpecifications(
        IEnumerable<(int BucketId, OrderEntity Order)> rawData,
        PaginationSpecification? paginationSpecification,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications)
    {
        if (sortingSpecifications is not null)
        {
            rawData = rawData.ApplySortings(sortingSpecifications);
        }

        return paginationSpecification is not null
            ? rawData.Take(paginationSpecification.PageSize)
            : rawData;
    }
    
    private string GetNextPageToken(IEnumerable<(int BucketId, OrderEntity Order)> resultData,
        IReadOnlyDictionary<int, int> currentOffsets)
    {
        var result = new int[AllBuckets.Count()];

        // Заполнение массива количеством элементов в каждом бакете
        foreach (var (bucketId, _) in resultData)
        {
            result[bucketId]++;
        }

        // Добавление текущих смещений
        for (var i = 0; i < result.Length; i++)
        {
            result[i] += currentOffsets.GetValueOrDefault(i, 0);
        }

        // Преобразование в Base64
        var bytes = new byte[result.Length * sizeof(int)];
        Buffer.BlockCopy(result, 0, bytes, 0, bytes.Length);
        return Convert.ToBase64String(bytes);
    }

    private List<string> ConfigureCommonFilteringConditions(
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications)
    {
        var filterConditions = new List<string>();

        if (filteringSpecifications is null || filteringSpecifications.Count is 0)
            return filterConditions;

        foreach (var filter in filteringSpecifications)
        {
            if (filter.RegionIdPossibleValues is not null && filter.RegionIdPossibleValues.Any())
            {
                var regionIds = string.Join(",", filter.RegionIdPossibleValues);
                filterConditions.Add($"o.region_id in ({regionIds})");
            }

            if (filter.MinCreateDateTime.HasValue)
            {
                filterConditions.Add($"o.create_date >= '{filter.MinCreateDateTime}'");
            }
        }

        return filterConditions;
    }

    private async Task<Dictionary<int, long[]>> GetBucketsAndOrdersToCheckByIndexes(
        CancellationToken cancellationToken,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null)
    {
        if (filteringSpecifications is null || filteringSpecifications.Count is 0)
        {
            return ReturnNoFilteringBucketsAndOrders();
        }

        var result = new Dictionary<int, long[]>();
        foreach (var filter in filteringSpecifications)
        {
            if (filter.CustomerIdPossibleValues is null || !filter.CustomerIdPossibleValues.Any())
                continue;

            var bucketsByCustomerIds = await GetBucketsAndOrdersToCheckByCustomerIds(cancellationToken, filter);

            foreach (var bucket in bucketsByCustomerIds)
            {
                result.Add(bucket.Key, bucket.Value);
            }
        }

        return result.Count is 0 ? ReturnNoFilteringBucketsAndOrders() : result;
    }

    private Dictionary<int, long[]> ReturnNoFilteringBucketsAndOrders()
    {
        return AllBuckets.ToDictionary<int, int, long[]>(bucket => bucket, _ => []);
    }

    private async Task<Dictionary<int, long[]>> GetBucketsAndOrdersToCheckByCustomerIds(CancellationToken cancellationToken,
        OrderFilteringSpecification filter)
    {
        var customerIds = string.Join(",", filter.CustomerIdPossibleValues!);
        var indexQuery = $"""
                              select order_id
                              from {CUSTOMER_INDEX_TABLE}
                              where customer_id in ({customerIds});
                          """;

        List<long> orderIds = [];
        var bucketsToCheck = filter.CustomerIdPossibleValues!
            .Select(GetBucketByShardKey)
            .Distinct();

        foreach (var bucket in bucketsToCheck)
        {
            await using var connection = GetConnectionByBucket(bucket);
            
            var command = new CommandDefinition(indexQuery, cancellationToken: cancellationToken);
            orderIds.AddRange(await connection.QueryAsync<long>(command));
        }

        var bucketToOrderIdsMap = orderIds
            .GroupBy(GetBucketByShardKey)
            .ToDictionary(
                g => g.Key,
                g => g.Select(id => id).ToArray());
        
        return bucketToOrderIdsMap;
    }

    public async Task InsertAsync(OrderDal order, bool shouldBeSentToOutbox, CancellationToken cancellationToken)
    {
        await using (var connection = GetConnectionByShardKey(order.Id))
        {
            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    IsolationLevel = IsolationLevel.ReadCommitted
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {

                var command = PrepareOrderInsertCommand(order, cancellationToken);
                await connection.QueryAsync(command);

                if (shouldBeSentToOutbox)
                {
                    var outboxCommand = PrepareNewOrderOutboxInsertCommand(order, cancellationToken);
                    await connection.QueryAsync(outboxCommand);
                }

                transaction.Complete();
            }
        }

        await UpdateCustomerIndex(order, cancellationToken);
    }

    private CommandDefinition PrepareNewOrderOutboxInsertCommand(OrderDal order, CancellationToken cancellationToken)
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

    private CommandDefinition PrepareOrderInsertCommand(OrderDal order, CancellationToken cancellationToken)
    {
        const string query =
            $"""
                 INSERT INTO {TABLE} ({FIELDS})
                 VALUES (@Id, @GoodsCount, @TotalAmount, @TotalWeight, @Type::order_type, @CreateDate,
                         @RegionId, @State::order_state, @CustomerId, @CustomerFirstName, @CustomerLastName,
                         @CustomerAddress::jsonb, @CustomerMobileNumber);
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
        parameters.Add("CustomerFirstName", order.CustomerFirstName);
        parameters.Add("CustomerLastName", order.CustomerLastName);
        parameters.Add("CustomerAddress", order.CustomerAddress);
        parameters.Add("CustomerMobileNumber", order.CustomerMobileNumber);

        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);
        return command;
    }

    private async Task UpdateCustomerIndex(OrderDal order, CancellationToken cancellationToken)
    {
        const string customerIdIndexQuery =
            $"""
                INSERT INTO {CUSTOMER_INDEX_TABLE} ({CUSTOMER_ID_INDEX_FIELDS})
                VALUES (@customerId, @orderId) ON CONFLICT DO NOTHING;
             """;

        await using var connection = GetConnectionByShardKey(order.CustomerId);
        var parameters = new DynamicParameters();
        parameters.Add("customerId", order.CustomerId);
        parameters.Add("orderId", order.Id);

        var command = new CommandDefinition(
            customerIdIndexQuery,
            parameters,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task UpdateAsync(OrderDal order, CancellationToken cancellationToken)
    {
        const string query =
            $"""
                UPDATE {TABLE}
                SET id = @Id, goods_count = @GoodsCount, total_amount = @TotalAmount, total_weight = @TotalWeight,
                    type = @Type::order_type, create_date = @CreateDate, region_id = @RegionId,
                    state = @State::order_state, customer_id = @CustomerId, customer_first_name= @CustomerFirstName,
                    customer_last_name = @CustomerLastName, customer_address = @CustomerAddress::jsonb,
                    customer_mobile_number = @CustomerMobileNumber
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
        parameters.Add("CustomerFirstName", order.CustomerFirstName);
        parameters.Add("CustomerLastName", order.CustomerLastName);
        parameters.Add("CustomerAddress", order.CustomerAddress);
        parameters.Add("CustomerMobileNumber", order.CustomerMobileNumber);

        var command = new CommandDefinition(
            query,
            parameters: parameters,
            cancellationToken: cancellationToken);

        await using var connection = GetConnectionByShardKey(order.Id);
        await connection.ExecuteAsync(command);
        
        await UpdateCustomerIndex(order, cancellationToken);
    }
}