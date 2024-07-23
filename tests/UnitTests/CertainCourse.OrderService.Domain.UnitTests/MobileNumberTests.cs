using CertainCourse.OrderService.Domain.Exceptions;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using Xunit;

namespace CertainCourse.OrderService.Domain.UnitTests;

public sealed class MobileNumberTests
{
    [Theory]
    [InlineData("+70123132132")]
    [InlineData("+7(0)123(132)132")]
    [InlineData("+70-1--2-31-32-132")]
    public void Create_ValidMobileNumber_ReturnsMobileNumberInstance(string mobileNumberValue)
    {
        // Act
        var mobileNumber = MobileNumber.Create(mobileNumberValue);
        
        // Assert
        Assert.NotNull(mobileNumber);
        Assert.Equal(mobileNumberValue, mobileNumber.Value);
    }
    
    [Theory]
    [InlineData("+7dfdf0123132132")]
    [InlineData("7_(0)123(132)132")]
    [InlineData("|")]
    [InlineData("")]
    public void Create_InvalidMobileNumber_ThrowsDomainException(string mobileNumberValue)
    {
        // Act
        var exception = Assert.Throws<DomainException>(() => MobileNumber.Create(mobileNumberValue));
        
        // Assert
        Assert.Equal($"Номер телефона `{mobileNumberValue}` содержит недопустимые символы", exception.Message);
    }
    
    [Fact]
    public void Equals_SameMobileNumberValue_ReturnsTrue()
    {
        // Arrange
        var mobileNumber1 = MobileNumber.Create("1234567890");
        var mobileNumber2 = MobileNumber.Create("1234567890");

        // Act & Assert
        Assert.True(mobileNumber1.Equals(mobileNumber2));
    }

    [Fact]
    public void Equals_DifferentMobileNumberValue_ReturnsFalse()
    {
        // Arrange
        var mobileNumber1 = MobileNumber.Create("1234567890");
        var mobileNumber2 = MobileNumber.Create("0987654321");

        // Act & Assert
        Assert.False(mobileNumber1.Equals(mobileNumber2));
    }

    [Fact]
    public void GetHashCode_SameMobileNumberValue_ReturnsSameHashCode()
    {
        // Arrange
        var mobileNumber1 = MobileNumber.Create("1234567890");
        var mobileNumber2 = MobileNumber.Create("1234567890");

        // Act & Assert
        Assert.Equal(mobileNumber1.GetHashCode(), mobileNumber2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentMobileNumberValue_ReturnsDifferentHashCode()
    {
        // Arrange
        var mobileNumber1 = MobileNumber.Create("1234567890");
        var mobileNumber2 = MobileNumber.Create("0987654321");

        // Act & Assert
        Assert.NotEqual(mobileNumber1.GetHashCode(), mobileNumber2.GetHashCode());
    }

    [Fact]
    public void ImplicitOperator_ReturnsMobileNumberValue()
    {
        // Arrange
        var mobileNumber = MobileNumber.Create("1234567890");
        
        // Act
        string mobileNumberValue = mobileNumber;

        // Assert
        Assert.Equal("1234567890", mobileNumberValue);
    }
}