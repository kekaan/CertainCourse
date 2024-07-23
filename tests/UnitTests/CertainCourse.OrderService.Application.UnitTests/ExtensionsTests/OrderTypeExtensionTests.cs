using CertainCourse.OrderService.Application.Extensions;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using Xunit;
using MessageBrokerOrderType = CertainCourse.OrderService.Application.NewPreOrder.OrderSource;

namespace CertainCourse.OrderService.Application.UnitTests.ExtensionsTests;

public class OrderTypeExtensionTests
{
    [Theory]
    [InlineData(MessageBrokerOrderType.Mobile, OrderType.Mobile)]
    [InlineData(MessageBrokerOrderType.WebSite, OrderType.Web)]
    [InlineData(MessageBrokerOrderType.Api, OrderType.Api)]
    public void ToDomain_ValidOrderType_ReturnsExpectedDomainType(MessageBrokerOrderType input, OrderType expected)
    {
        // Act
        var result = input.ToDomain();
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDomain_InvalidOrderType_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var invalidType = (MessageBrokerOrderType)999;
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => invalidType.ToDomain());
    }
}