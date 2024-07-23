namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record GetOrdersAggregatedByRegionRequestDto(
    DateTime StartTime,
    IReadOnlyCollection<int> RegionsIds);