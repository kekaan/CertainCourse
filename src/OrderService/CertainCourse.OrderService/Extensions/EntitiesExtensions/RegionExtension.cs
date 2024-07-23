using CertainCourse.OrderService.Domain;
using RegionProto = CertainCourse.Orders.Grpc.Region;

namespace CertainCourse.OrderService.Extensions.EntitiesExtensions;

internal static class RegionExtension
{
    public static RegionProto ToRegionProto(this RegionEntity region)
    {
        return new RegionProto
        {
            Id = region.Id,
            Name = region.Name
        };
    }
}