namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record CancelOrderByIdResponseDto(bool IsSuccess, string? ErrorMessage);