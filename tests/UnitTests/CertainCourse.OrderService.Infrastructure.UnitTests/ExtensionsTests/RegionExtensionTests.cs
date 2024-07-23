using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.ExtensionsTests;

public class RegionExtensionTests
{
    [Fact]
    public void ToDomain_ValidRegionDalAndStorageDal_ReturnsExpectedRegionEntity()
    {
        // Arrange
        var regionDal = new RegionDal
        {
            Id = 1,
            Name = "Region"
        };

        var storageDal = new StorageDal
        {
            Latitude = 1.23,
            Longitude = 4.56
        };

        // Act
        var result = regionDal.ToDomain(storageDal);

        // Assert
        Assert.Equal(regionDal.Id, result.Id.Value);
        Assert.Equal(regionDal.Name, result.Name);
        Assert.Equal(storageDal.Latitude, result.Storage.Latitude);
        Assert.Equal(storageDal.Longitude, result.Storage.Longitude);
    }
}