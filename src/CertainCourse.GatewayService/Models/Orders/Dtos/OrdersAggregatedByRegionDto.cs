namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record OrdersAggregatedByRegionDto(
    string Region,
    long OrdersCount,
    decimal OrdersTotalAmount,
    double OrdersTotalWeight,
    int CustomersCount);