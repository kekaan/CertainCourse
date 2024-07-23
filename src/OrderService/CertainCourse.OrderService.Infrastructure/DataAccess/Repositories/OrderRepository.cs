using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly IOrderDbRepository _orderDbRepository;
    private readonly IRegionDbRepository _regionDbRepository;

    public OrderRepository(IOrderDbRepository orderDbRepository, IRegionDbRepository regionDbRepository)
    {
        _orderDbRepository = orderDbRepository;
        _regionDbRepository = regionDbRepository;
    }

    public async Task<OrderEntity> GetOrderByIdAsync(long id,
        CancellationToken cancellationToken, OrderIncludeSpecification? includeSpecification = null)
    {
        var needInclude = includeSpecification is not null;

        var order = await _orderDbRepository.GetByIdAsync(id, cancellationToken);
        var region = needInclude && includeSpecification!.IncludeRegionInfo
            ? await _regionDbRepository.GetByIdAsync(order.RegionId, cancellationToken)
            : null;

        return order.ToDomain(region);
    }

    public async Task CreateOrderAsync(OrderEntity order, bool shouldBeSentToOutbox,
        CancellationToken cancellationToken)
    {
        await _orderDbRepository.InsertAsync(order.ToDal(), shouldBeSentToOutbox, cancellationToken);
    }

    public async Task UpdateOrderAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        await _orderDbRepository.UpdateAsync(order.ToDal(), cancellationToken);
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
        await _orderDbRepository.UpdateAsync(newOrder.ToDal(), cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderEntity>> GetOrdersAsync(CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        var ordersPagedCollection = await GetOrdersInternalAsync(cancellationToken,
            includeSpecification: includeSpecification,
            filteringSpecifications: filteringSpecifications,
            sortingSpecifications: sortingSpecifications);

        return ordersPagedCollection.Items;
    }

    public Task<PagedCollection<OrderEntity>> GetOrdersPagedListAsync(
        PaginationSpecification paginationSpecification,
        CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        return GetOrdersInternalAsync(cancellationToken,
            includeSpecification: includeSpecification,
            paginationSpecification: paginationSpecification,
            filteringSpecifications: filteringSpecifications,
            sortingSpecifications: sortingSpecifications);
    }

    private async Task<PagedCollection<OrderEntity>> GetOrdersInternalAsync(CancellationToken cancellationToken,
        PaginationSpecification? paginationSpecification = null,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null)
    {
        var orders = await _orderDbRepository.GetAllAsync(cancellationToken,
            paginationSpecification: paginationSpecification,
            filteringSpecifications: filteringSpecifications,
            sortingSpecifications: sortingSpecifications);

        return orders;
    }
}