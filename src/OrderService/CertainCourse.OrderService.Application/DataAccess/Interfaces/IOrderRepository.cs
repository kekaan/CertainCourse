using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;

namespace CertainCourse.OrderService.Application.DataAccess.Interfaces;

public interface IOrderRepository
{
    Task<OrderEntity> GetOrderByIdAsync(long id, CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null);

    public Task CreateOrderAsync(OrderEntity order, bool shouldBeSentToOutbox, CancellationToken cancellationToken);

    public Task UpdateOrderAsync(OrderEntity order, CancellationToken cancellationToken);

    public Task CancelOrderAsync(long orderId, CancellationToken cancellationToken);

    public Task<IReadOnlyCollection<OrderEntity>> GetOrdersAsync(
        CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null);

    public Task<PagedCollection<OrderEntity>> GetOrdersPagedListAsync(
        PaginationSpecification paginationSpecification,
        CancellationToken cancellationToken,
        OrderIncludeSpecification? includeSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null);
}