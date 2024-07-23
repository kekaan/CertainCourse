using CertainCourse.OrderService.Application.NewPreOrder;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Order;

namespace CertainCourse.OrderService.Application.Extensions;

internal static class OrderExtension
{
    public static OrderEntity ToDomain(this NewPreOrderEvent request, RegionEntity region, CustomerEntity customer)
    {
        return new OrderEntity
        (
            Id: new OrderId(request.Id),
            GoodsCount: request.Goods.Count(),
            TotalAmount: request.Goods.Sum(e => e.Price * e.Quantity),
            TotalWeight: request.Goods.Sum(e => e.Weight * e.Quantity),
            Type: request.Source.ToDomain(),
            CreateDate: DateTime.UtcNow,
            RegionId: region.Id,
            RegionName: region.Name,
            State: OrderState.Created,
            CustomerId: customer.Id,
            CustomerFirstName: customer.FirstName,
            CustomerLastName: customer.LastName,
            CustomerMobileNumber: customer.MobileNumber,
            CustomerAddress: request.Customer.Address.ToDomain());
    }
}