namespace CertainCourse.OrderService.Domain.ValueObjects.Region;

public readonly record struct RegionId(int Value)
{
    public static implicit operator int(RegionId regionId) => regionId.Value;
}