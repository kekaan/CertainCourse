using CertainCourse.OrderService.Application.Extensions;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using Xunit;
using MessageBrokerOrderState = CertainCourse.OrderService.Application.OrderStateChanged.OrderState;

namespace CertainCourse.OrderService.Application.UnitTests.ExtensionsTests;

public class OrderStateExtensionTests
{
    [Theory]
    [InlineData(MessageBrokerOrderState.Created, OrderState.Created)]
    [InlineData(MessageBrokerOrderState.SentToCustomer, OrderState.SentToCustomer)]
    [InlineData(MessageBrokerOrderState.Delivered, OrderState.Delivered)]
    [InlineData(MessageBrokerOrderState.Lost, OrderState.Lost)]
    [InlineData(MessageBrokerOrderState.Cancelled, OrderState.Cancelled)]
    public void ToDomain_ValidOrderState_ReturnsExpectedDomainState(MessageBrokerOrderState input, OrderState expected)
    {
        // Act
        var result = input.ToDomain();
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDomain_InvalidOrderState_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var invalidState = (MessageBrokerOrderState)999;
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => invalidState.ToDomain());
    }
}