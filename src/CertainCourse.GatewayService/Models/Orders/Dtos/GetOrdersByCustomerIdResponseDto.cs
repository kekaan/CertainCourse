namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record GetOrdersByCustomerIdResponseDto(IReadOnlyCollection<OrderDto> Orders, string NextPageToken);