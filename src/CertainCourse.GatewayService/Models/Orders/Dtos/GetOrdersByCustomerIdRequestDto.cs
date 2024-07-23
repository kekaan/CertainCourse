namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record GetOrdersByCustomerIdRequestDto(
    int CustomerId,
    DateTime StartTime,
    int PageSize,
    string? PageToken);