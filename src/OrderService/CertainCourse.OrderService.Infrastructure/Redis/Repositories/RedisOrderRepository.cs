using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.DataAccess.Common.Extensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.Redis.Repositories;
using StackExchange.Redis;

namespace CertainCourse.OrderService.DataAccess.Repositories.RedisRepositories;

internal sealed class RedisOrderRepository : BaseRedisRepository, IOrderRepository
{
    private const string ORDER_KEY_PREFIX = "orders";

    public RedisOrderRepository(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> redisSettings) :
        base(connectionMultiplexer, redisSettings, ORDER_KEY_PREFIX)
    {
    }

    public async Task<OrderEntity> GetOrderByIdAsync(long id, CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(id);

        await IsKeyExistsOrThrow(key);

        var resultRedis = await _database.StringGetAsync(key);

        var result = ToObject<OrderEntity>(resultRedis);

        return result;
    }

    public async Task CreateOrderAsync(OrderEntity order, bool shouldBeSentToOutbox, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(order.Id);

        await IsKeyNotExistsOrThrow(key);

        var resultRedis = ToRedisString(order);

        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task UpdateOrderAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(order.Id);

        await IsKeyExistsOrThrow(key);

        var resultRedis = ToRedisString(order);
        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task CancelOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderByIdAsync(orderId, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (order.State is OrderState.Cancelled or OrderState.Delivered)
        {
            throw new InvalidOperationException(
                $"Order with this id is already delivered or cancelled: {orderId}");
        }

        var newOrder = order with { State = OrderState.Cancelled };

        var key = BuildKey(newOrder.Id);
        var resultRedis = ToRedisString(newOrder);
        await _database.StringSetAsync(key, resultRedis);
    }

    public Task<IReadOnlyCollection<OrderEntity>> GetOrdersAsync(CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        return GetOrdersInternalAsync(cancellationToken, filteringSpecifications, sortingSpecifications);
    }

    public async Task<PagedCollection<OrderEntity>> GetOrdersPagedListAsync(
        PaginationSpecification paginationSpecification,
        CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        var orders = await GetOrdersInternalAsync(
            cancellationToken, filteringSpecifications, sortingSpecifications, paginationSpecification);

        return orders.ToPagedList(paginationSpecification.PageSize,
            paginationSpecification.PageToken,
            paginationSpecification.PageToken + 1);
    }

    private async Task<IReadOnlyCollection<OrderEntity>> GetOrdersInternalAsync(
        CancellationToken cancellationToken,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null,
        PaginationSpecification? paginationSpecification = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keys = _server.KeysAsync(database: _db, pattern: $"{ORDER_KEY_PREFIX}*");
        var ordersFromRedis = new ConcurrentBag<OrderEntity>();

        await foreach (var redisKey in keys)
        {
            var orderRedis = await _database.StringGetAsync(redisKey);
            var order = ToObject<OrderEntity>(orderRedis.ToString());

            ordersFromRedis.Add(order);
        }

        IEnumerable<OrderEntity> query = ordersFromRedis;
        if (filteringSpecifications is not null)
        {
            query = query.ApplyFilters(filteringSpecifications);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (sortingSpecifications is not null)
        {
            query = query.ApplySortings(sortingSpecifications);
        }

        return paginationSpecification is not null
            ? query.Skip(int.Parse(paginationSpecification.PageToken) * paginationSpecification.PageSize)
                .Take(paginationSpecification.PageSize).ToArray()
            : query.ToArray();
    }
}