using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Order;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

internal sealed record OrderDal
{
    public long Id { get; init; }
    public int GoodsCount { get; init; }
    public decimal TotalAmount { get; init; }
    public double TotalWeight { get; init; }
    public OrderType Type { get; init; }
    public DateTime CreateDate { get; init; }
    public int RegionId { get; init; }
    public OrderState State { get; init; }
    public int CustomerId { get; init; }
    public string CustomerFirstName { get; init; } = null!;
    public string CustomerLastName { get; init; } = null!;
    public Address CustomerAddress { get; init; } = null!;
    public string CustomerMobileNumber { get; init; } = null!;
}