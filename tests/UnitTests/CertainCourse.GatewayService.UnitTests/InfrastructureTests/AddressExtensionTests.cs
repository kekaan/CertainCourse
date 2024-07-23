using CertainCourse.GatewayService.Infrastructure.Extensions;
using CertainCourse.GatewayService.Models.Common;

namespace CertainCourse.GatewayService.Tests.InfrastructureTests;

public class AddressExtensionTests
{
    [Fact]
    public void ToAddress_ShouldConvertOrdersAddressCorrectly()
    {
        // Arrange
        var grpcAddress = new Orders.Grpc.Address
        {
            Region = "Region",
            City = "City",
            Street = "Street",
            Building = "Building",
            Apartment = "Apartment",
            Latitude = 55.7558,
            Longitude = 37.6176
        };

        // Act
        AddressDto addressDto = grpcAddress.ToAddress();

        // Assert
        Assert.Equal("Region", addressDto.Region);
        Assert.Equal("City", addressDto.City);
        Assert.Equal("Street", addressDto.Street);
        Assert.Equal("Building", addressDto.Building);
        Assert.Equal("Apartment", addressDto.Apartment);
        Assert.Equal(55.7558, addressDto.Latitude);
        Assert.Equal(37.6176, addressDto.Longitude);
    }

    [Fact]
    public void ToAddress_ShouldConvertCustomersAddressCorrectly()
    {
        // Arrange
        var grpcAddress = new Customers.Grpc.Address
        {
            Region = "Region",
            City = "City",
            Street = "Street",
            Building = "Building",
            Apartment = "Apartment",
            Latitude = 55.7558,
            Longitude = 37.6176
        };

        // Act
        AddressDto addressDto = grpcAddress.ToAddress();

        // Assert
        Assert.Equal("Region", addressDto.Region);
        Assert.Equal("City", addressDto.City);
        Assert.Equal("Street", addressDto.Street);
        Assert.Equal("Building", addressDto.Building);
        Assert.Equal("Apartment", addressDto.Apartment);
        Assert.Equal(55.7558, addressDto.Latitude);
        Assert.Equal(37.6176, addressDto.Longitude);
    }
}