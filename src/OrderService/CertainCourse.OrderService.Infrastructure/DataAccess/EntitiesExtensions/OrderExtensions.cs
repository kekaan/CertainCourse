using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

internal static class OrderExtension
{
    public static OrderEntity ToDomain(this OrderDal order, RegionDal? region)
    {
        return new OrderEntity(
            Id: new OrderId(order.Id),
            GoodsCount: order.GoodsCount,
            TotalAmount: order.TotalAmount,
            TotalWeight: order.TotalWeight,
            Type: order.Type,
            CreateDate: order.CreateDate,
            RegionId: new RegionId(order.RegionId),
            RegionName: region?.Name,
            State: order.State,
            CustomerId: new CustomerId(order.CustomerId),
            CustomerFirstName: order.CustomerFirstName,
            CustomerLastName: order.CustomerLastName,
            CustomerMobileNumber: MobileNumber.Create( order.CustomerMobileNumber),
            CustomerAddress: order.CustomerAddress);
    }

    public static OrderDal ToDal(this OrderEntity order)
    {
        return new OrderDal
        {
            Id = order.Id,
            GoodsCount = order.GoodsCount,
            TotalAmount = order.TotalAmount,
            TotalWeight = order.TotalWeight,
            Type = order.Type,
            CreateDate = order.CreateDate,
            RegionId = order.RegionId,
            State = order.State,
            CustomerId = order.CustomerId,
            CustomerFirstName = order.CustomerFirstName,
            CustomerLastName = order.CustomerLastName,
            CustomerAddress = order.CustomerAddress,
            CustomerMobileNumber = order.CustomerMobileNumber
        };
    }
    
    public static string ToDatabaseColumn(this OrderProperty property)
    {
        return property switch
        {
            OrderProperty.Id => "id",
            OrderProperty.Type => "type",
            OrderProperty.CreateDate => "create_date",
            OrderProperty.RegionId => "region_id",
            OrderProperty.State => "state",
            OrderProperty.CustomerId => "customer_id",
            _ => throw new ArgumentException($"Such sorting is not supported: {property}")
        };
    }

    public static IEnumerable<OrderEntity> ApplyFilters(this IEnumerable<OrderEntity> query,
        IReadOnlyCollection<OrderFilteringSpecification> filterSpecifications)
    {
        foreach (var filterSpecification in filterSpecifications)
        {
            query = query.ApplyFilter(filterSpecification);
        }

        return query;
    }

    private static IEnumerable<OrderEntity> ApplyFilter(this IEnumerable<OrderEntity> query,
        OrderFilteringSpecification filterSpecification)
    {
        if (filterSpecification.MinCreateDateTime is not null)
        {
            query = query.Where(o => o.CreateDate >= filterSpecification.MinCreateDateTime);
        }

        if (filterSpecification.RegionIdPossibleValues is not null)
        {
            query = query.Where(o => filterSpecification.RegionIdPossibleValues.Contains(o.RegionId));
        }

        if (filterSpecification.CustomerIdPossibleValues is not null)
        {
            query = query.Where(o => filterSpecification.CustomerIdPossibleValues.Contains(o.CustomerId));
        }

        return query;
    }

    public static IEnumerable<OrderEntity> ApplySortings(this IEnumerable<OrderEntity> query,
        IReadOnlyCollection<OrderSortingSpecification> sortingSpecifications)
    {
        foreach (var sortingSpecification in sortingSpecifications)
        {
            query = query.ApplySorting(sortingSpecification);
        }

        return query;
    }

    private static IEnumerable<OrderEntity> ApplySorting(this IEnumerable<OrderEntity> query,
        OrderSortingSpecification sortingSpecification)
    {
        query = sortingSpecification.Property switch
        {
            OrderProperty.Id => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.Id)
                : query.OrderBy(o => o.Id),
            OrderProperty.Type => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.Type)
                : query.OrderBy(o => o.Type),
            OrderProperty.CreateDate => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.CreateDate)
                : query.OrderBy(o => o.CreateDate),
            OrderProperty.RegionId => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.RegionId)
                : query.OrderBy(o => o.RegionId),
            OrderProperty.State => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.State)
                : query.OrderBy(o => o.State),
            OrderProperty.CustomerId => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.CustomerId)
                : query.OrderBy(o => o.CustomerId),
            _ => throw new ArgumentException($"Such sorting is not supported: {sortingSpecification.Property}")
        };

        return query;
    }

    public static IEnumerable<(int BucketId, OrderEntity Order)> ApplySortings(
        this IEnumerable<(int BucketId, OrderEntity Order)> query,
        IReadOnlyCollection<OrderSortingSpecification> sortingSpecifications)
    {
        foreach (var sortingSpecification in sortingSpecifications)
        {
            query = query.ApplySorting(sortingSpecification);
        }

        return query;
    }

    private static IEnumerable<(int BucketId, OrderEntity Order)> ApplySorting(
        this IEnumerable<(int BucketId, OrderEntity Order)> query,
        OrderSortingSpecification sortingSpecification)
    {
        query = sortingSpecification.Property switch
        {
            OrderProperty.Id => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.Order.Id)
                : query.OrderBy(o => o.Order.Id),
            OrderProperty.Type => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.Order.Type)
                : query.OrderBy(o => o.Order.Type),
            OrderProperty.CreateDate => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.Order.CreateDate)
                : query.OrderBy(o => o.Order.CreateDate),
            OrderProperty.RegionId => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => (int)o.Order.RegionId)
                : query.OrderBy(o => (int)o.Order.RegionId),
            OrderProperty.State => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => o.Order.State)
                : query.OrderBy(o => o.Order.State),
            OrderProperty.CustomerId => sortingSpecification.SortDescending
                ? query.OrderByDescending(o => (int)o.Order.CustomerId)
                : query.OrderBy(o => (int)o.Order.CustomerId),
            _ => throw new ArgumentException($"Such sorting is not supported: {sortingSpecification.Property}")
        };

        return query;
    }
}