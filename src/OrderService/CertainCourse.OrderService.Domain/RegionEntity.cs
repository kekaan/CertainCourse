using CertainCourse.OrderService.Domain.ValueObjects.Region;

namespace CertainCourse.OrderService.Domain;

public sealed record RegionEntity(RegionId Id, string Name, Storage Storage);