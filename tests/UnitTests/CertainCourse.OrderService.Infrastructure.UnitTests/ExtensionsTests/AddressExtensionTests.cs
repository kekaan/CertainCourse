using Xunit;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CustomersAddressProto = CertainCourse.Customers.Grpc.Address;
using AddressEntity = CertainCourse.OrderService.Domain.ValueObjects.Customer.Address;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.ExtensionsTests;

public class AddressExtensionTests
{
    [Fact]
    public void ToDomain_ValidAddressProto_ReturnsExpectedAddressEntity()
    {
        // Arrange
        var addressProto = new CustomersAddressProto
        {
            Region = "Region",
            City = "City",
            Street = "Street",
            Building = "Building",
            Apartment = "Apartment",
            Latitude = 1.23,
            Longitude = 4.56
        };

        var expected = new AddressEntity
        {
            Region = "Region",
            City = "City",
            Street = "Street",
            Building = "Building",
            Apartment = "Apartment",
            Latitude = 1.23,
            Longitude = 4.56
        };

        // Act
        var result = addressProto.ToDomain();
        
        // Assert
        Assert.Equal(expected.Region, result.Region);
        Assert.Equal(expected.City, result.City);
        Assert.Equal(expected.Street, result.Street);
        Assert.Equal(expected.Building, result.Building);
        Assert.Equal(expected.Apartment, result.Apartment);
        Assert.Equal(expected.Latitude, result.Latitude);
        Assert.Equal(expected.Longitude, result.Longitude);
    }
}