using CertainCourse.OrderService.Domain.Exceptions;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using Xunit;

namespace CertainCourse.OrderService.Domain.UnitTests;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user@domain.com")]
    public void Create_ValidEmail_ReturnsEmailInstance(string emailValue)
    {
        // Act
        var email = Email.Create(emailValue);
        
        // Assert
        Assert.NotNull(email);
        Assert.Equal(emailValue, email.Value);
    }

    [Theory]
    [InlineData("testexample.com")]
    [InlineData("userdomain.com")]
    public void Create_InvalidEmail_ThrowsDomainException(string emailValue)
    {
        // Act
        var exception = Assert.Throws<DomainException>(() => Email.Create(emailValue));
        
        // Assert
        Assert.Equal($"Email `{emailValue}` не содержит символ `@`", exception.Message);
    }

    [Fact]
    public void Equals_SameEmailValue_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        Assert.True(email1.Equals(email2));
    }

    [Fact]
    public void Equals_DifferentEmailValue_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("user@domain.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
    }

    [Fact]
    public void GetHashCode_SameEmailValue_ReturnsSameHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentEmailValue_ReturnsDifferentHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("user@domain.com");

        // Act & Assert
        Assert.NotEqual(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void ImplicitOperator_ReturnsEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        
        // Act
        string emailValue = email;

        // Assert
        Assert.Equal("test@example.com", emailValue);
    }
}