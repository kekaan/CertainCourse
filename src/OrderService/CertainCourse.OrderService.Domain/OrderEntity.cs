using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Domain.ValueObjects.Region;

namespace CertainCourse.OrderService.Domain;

public sealed record OrderEntity(
    OrderId Id,
    int GoodsCount,
    decimal TotalAmount,
    double TotalWeight,
    OrderType Type,
    DateTime CreateDate,
    RegionId RegionId,
    string? RegionName,
    OrderState State,
    CustomerId CustomerId,
    string CustomerFirstName,
    string CustomerLastName,
    MobileNumber CustomerMobileNumber,
    Address CustomerAddress);