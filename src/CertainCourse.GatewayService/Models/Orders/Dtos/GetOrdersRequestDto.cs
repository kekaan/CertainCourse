namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record GetOrdersRequestDto(
    int RegionId,
    int PageSize,
    string? PageToken,
    string? OrderBy);