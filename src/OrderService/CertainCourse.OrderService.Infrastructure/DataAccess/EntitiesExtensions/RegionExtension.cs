using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

internal static class RegionExtension
{
    public static RegionEntity ToDomain(this RegionDal regionDal, StorageDal storageDal)
    {
        return new RegionEntity(
            Id: new RegionId(regionDal.Id),
            Name: regionDal.Name,
            Storage: new Storage(storageDal.Latitude, storageDal.Longitude));
    }
}