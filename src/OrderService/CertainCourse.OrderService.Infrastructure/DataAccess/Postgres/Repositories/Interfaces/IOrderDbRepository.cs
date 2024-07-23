using CertainCourse.OrderService.Common;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;

internal interface IOrderDbRepository
{
    Task<OrderDal> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<PagedCollection<OrderEntity>> GetAllAsync(CancellationToken cancellationToken,
        PaginationSpecification? paginationSpecification = null,
        IReadOnlyCollection<OrderFilteringSpecification>? filteringSpecifications = null,
        IReadOnlyCollection<OrderSortingSpecification>? sortingSpecifications = null);

    Task InsertAsync(OrderDal order, bool shouldBeSentToOutbox, CancellationToken cancellationToken);
    
    Task UpdateAsync(OrderDal order, CancellationToken cancellationToken);
}