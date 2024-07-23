using CertainCourse.Orders.Grpc;
using CertainCourse.OrderService.Domain;

namespace CertainCourse.OrderService.Extensions.EntitiesExtensions;

internal static class OrderExtension
{
    public static Order ToProto(this OrderEntity order)
    {
        return new Order
        {
            Id = order.Id,
            GoodsCount = order.GoodsCount,
            TotalAmount = order.TotalAmount,
            TotalWeight = order.TotalWeight,
            Type = order.Type.ToProto(),
            CreateDate = UnixTimestampConverter.DateTimeToUnixTimeStamp(order.CreateDate),
            Region = order.RegionName,
            State = order.State.ToOrderStateProto(),
            CustomerFirstName = order.CustomerFirstName,
            CustomerLastName = order.CustomerLastName,
            Address = order.CustomerAddress.ToProto(),
            CustomerMobileNumber = order.CustomerMobileNumber
        };
    }

    public static OrdersAggregatedByRegion ToOrdersAggregatedByRegionProto(
        this IEnumerable<OrderEntity> orders)
    {
        return new OrdersAggregatedByRegion
        {
            Region = orders.First().RegionName,
            OrdersCount = orders.Count(),
            OrdersTotalAmount = orders.Sum(o => o.TotalAmount),
            OrdersTotalWeight = orders.Sum(o => o.TotalWeight),
            CustomersCount = orders
                .Select(o => o.CustomerId)
                .Distinct()
                .Count()
        };
    }
}