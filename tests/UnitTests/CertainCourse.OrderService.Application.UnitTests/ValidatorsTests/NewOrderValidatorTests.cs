using CertainCourse.OrderService.Application.Validators.Orders;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using Xunit;

namespace CertainCourse.OrderService.Application.UnitTests.ValidatorsTests;

public class NewOrderValidatorTests
{
    private readonly NewOrderValidator _validator;

    public NewOrderValidatorTests()
    {
        _validator = new NewOrderValidator();
    }

    [Theory]
    [InlineData(55.7558, 37.6176, 55.7558, 37.6176, false)] // Same coordinates (distance should be 0)
    [InlineData(55.7558, 37.6176, 59.9343, 30.3351, true)] // Moscow to St. Petersburg (distance should be greater than 5000 meters)
    [InlineData(55.7558, 37.6176, 55.7512, 37.6185, false)] // Small distance within 5000 meters
    public void IsValidByDistance_ValidCoordinates_ReturnsCorrectValidationResult(double lat1, double lon1, double lat2,
        double lon2, bool expectedResult)
    {
        // Arrange
        var address = new Address
        {
            Region = "Region",
            City = "City",
            Street = "Street",
            Building = "Building",
            Apartment = "Apartment",
            Latitude = lat1,
            Longitude = lon1
        };
        
        // Act
        var storage = new Storage(lat2, lon2);

        // Assert
        var result = _validator.IsValidByDistance(address, storage);
        Assert.Equal(expectedResult, result);
    }
}