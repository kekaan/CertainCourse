using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.ExtensionsTests;

public class OrderExtensionTests
{
    [Fact]
    public void ToDomain_ValidOrderDal_ReturnsExpectedOrderEntity()
    {
        // Arrange
        var orderDal = new OrderDal
        {
            Id = 1,
            GoodsCount = 2,
            TotalAmount = 100,
            TotalWeight = 1.5,
            Type = OrderType.Mobile,
            CreateDate = DateTime.Now,
            RegionId = 1,
            State = OrderState.Created,
            CustomerId = 1,
            CustomerFirstName = "John",
            CustomerLastName = "Doe",
            CustomerMobileNumber = "1234567890",
            CustomerAddress = new Address
            {
                Region = "Region",
                City = "City",
                Street = "Street",
                Building = "Building",
                Apartment = "Apartment",
                Latitude = 1.23,
                Longitude = 4.56
            }
        };

        var regionDal = new RegionDal
        {
            Id = 1,
            Name = "Region"
        };

        // Act
        var result = orderDal.ToDomain(regionDal);

        // Assert
        Assert.Equal(orderDal.Id, result.Id.Value);
        Assert.Equal(orderDal.GoodsCount, result.GoodsCount);
        Assert.Equal(orderDal.TotalAmount, result.TotalAmount);
        Assert.Equal(orderDal.TotalWeight, result.TotalWeight);
        Assert.Equal(orderDal.Type, result.Type);
        Assert.Equal(orderDal.CreateDate, result.CreateDate);
        Assert.Equal(orderDal.RegionId, result.RegionId.Value);
        Assert.Equal(orderDal.State, result.State);
        Assert.Equal(orderDal.CustomerId, result.CustomerId.Value);
        Assert.Equal(orderDal.CustomerFirstName, result.CustomerFirstName);
        Assert.Equal(orderDal.CustomerLastName, result.CustomerLastName);
        Assert.Equal(orderDal.CustomerMobileNumber, result.CustomerMobileNumber.Value);
        Assert.Equal(orderDal.CustomerAddress.Region, result.CustomerAddress.Region);
    }

    [Fact]
    public void ToDal_ValidOrderEntity_ReturnsExpectedOrderDal()
    {
        // Arrange
        var orderEntity = new OrderEntity(
            new OrderId(1),
            2,
            100,
            1.5,
            OrderType.Mobile,
            DateTime.Now,
            new RegionId(1),
            "Region",
            OrderState.Created,
            new CustomerId(1),
            "John",
            "Doe",
            MobileNumber.Create("1234567890"),
            new Address
            {
                Region = "Region",
                City = "City",
                Street = "Street",
                Building = "Building",
                Apartment = "Apartment",
                Latitude = 1.23,
                Longitude = 4.56
            }
        );

        // Act
        var result = orderEntity.ToDal();

        // Assert
        Assert.Equal(orderEntity.Id.Value, result.Id);
        Assert.Equal(orderEntity.GoodsCount, result.GoodsCount);
        Assert.Equal(orderEntity.TotalAmount, result.TotalAmount);
        Assert.Equal(orderEntity.TotalWeight, result.TotalWeight);
        Assert.Equal(orderEntity.Type, result.Type);
        Assert.Equal(orderEntity.CreateDate, result.CreateDate);
        Assert.Equal(orderEntity.RegionId.Value, result.RegionId);
        Assert.Equal(orderEntity.State, result.State);
        Assert.Equal(orderEntity.CustomerId.Value, result.CustomerId);
        Assert.Equal(orderEntity.CustomerFirstName, result.CustomerFirstName);
        Assert.Equal(orderEntity.CustomerLastName, result.CustomerLastName);
        Assert.Equal(orderEntity.CustomerMobileNumber.Value, result.CustomerMobileNumber);
        Assert.Equal(orderEntity.CustomerAddress.Region, result.CustomerAddress.Region);
    }
}