namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record GetOrdersResponseDto(IReadOnlyCollection<OrderDto> Orders, string NextPageToken);