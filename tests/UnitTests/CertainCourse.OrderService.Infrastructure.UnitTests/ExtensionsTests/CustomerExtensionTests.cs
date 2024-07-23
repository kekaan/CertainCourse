using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CustomerProto = CertainCourse.Customers.Grpc.Customer;
using CustomersAddressProto = CertainCourse.Customers.Grpc.Address;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.ExtensionsTests;

public class CustomerExtensionTests
{
    [Fact]
    public void ToDomain_ValidCustomerProto_ReturnsExpectedCustomerEntity()
    {
        // Arrange
        var customerProto = new CustomerProto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            MobileNumber = "1234567890",
            Email = "john.doe@example.com",
            DefaultAddress = new CustomersAddressProto
            {
                Region = "Region",
                City = "City",
                Street = "Street",
                Building = "Building",
                Apartment = "Apartment",
                Latitude = 1.23,
                Longitude = 4.56
            },
            Addressed =
            {
                new CustomersAddressProto
                {
                    Region = "Region2",
                    City = "City2",
                    Street = "Street2",
                    Building = "Building2",
                    Apartment = "Apartment2",
                    Latitude = 7.89,
                    Longitude = 10.11
                }
            }
        };

        // Act
        var result = customerProto.ToDomain();

        // Assert
        Assert.Equal(customerProto.Id, result.Id.Value);
        Assert.Equal(customerProto.FirstName, result.FirstName);
        Assert.Equal(customerProto.LastName, result.LastName);
        Assert.Equal(customerProto.MobileNumber, result.MobileNumber.Value);
        Assert.Equal(customerProto.Email, result.Email.Value);
        Assert.Equal(customerProto.DefaultAddress.Region, result.DefaultAddress.Region);
        Assert.Single(result.Addresses);
        Assert.Equal(customerProto.Addressed.First().Region, result.Addresses.First().Region);
    }

    [Fact]
    public void ToDomain_ValidCustomerDal_ReturnsExpectedCustomerEntity()
    {
        // Arrange
        var customerDal = new CustomerExtension.CustomerDal
        (
            Id: 1,
            FirstName: "John",
            LastName: "Doe",
            MobileNumber: "1234567890",
            Email: "john.doe@example.com",
            DefaultAddress: new Address
            {
                Region = "Region",
                City = "City",
                Street = "Street",
                Building = "Building",
                Apartment = "Apartment",
                Latitude = 1.23,
                Longitude = 4.56
            },
            Addresses:
            [
                new Address
                {
                    Region = "Region2",
                    City = "City2",
                    Street = "Street2",
                    Building = "Building2",
                    Apartment = "Apartment2",
                    Latitude = 7.89,
                    Longitude = 10.11
                }
            ]
        );

        // Act
        var result = customerDal.ToDomain();

        // Assert
        Assert.Equal(customerDal.Id, result.Id.Value);
        Assert.Equal(customerDal.FirstName, result.FirstName);
        Assert.Equal(customerDal.LastName, result.LastName);
        Assert.Equal(customerDal.MobileNumber, result.MobileNumber.Value);
        Assert.Equal(customerDal.Email, result.Email.Value);
        Assert.Equal(customerDal.DefaultAddress.Region, result.DefaultAddress.Region);
        Assert.Single(result.Addresses);
        Assert.Equal(customerDal.Addresses.First().Region, result.Addresses.First().Region);
    }

    [Fact]
    public void ToDal_ValidCustomerEntity_ReturnsExpectedCustomerDal()
    {
        // Arrange
        var customerEntity = new CustomerEntity(
            new CustomerId(1),
            "John",
            "Doe",
            MobileNumber.Create("1234567890"),
            Email.Create("john.doe@example.com"),
            new Address
            {
                Region = "Region",
                City = "City",
                Street = "Street",
                Building = "Building",
                Apartment = "Apartment",
                Latitude = 1.23,
                Longitude = 4.56
            },
            [
                new Address
                {
                    Region = "Region2",
                    City = "City2",
                    Street = "Street2",
                    Building = "Building2",
                    Apartment = "Apartment2",
                    Latitude = 7.89,
                    Longitude = 10.11
                }
            ]
        );

        // Act
        var result = customerEntity.ToDal();

        // Assert
        Assert.Equal(customerEntity.Id.Value, result.Id);
        Assert.Equal(customerEntity.FirstName, result.FirstName);
        Assert.Equal(customerEntity.LastName, result.LastName);
        Assert.Equal(customerEntity.MobileNumber.Value, result.MobileNumber);
        Assert.Equal(customerEntity.Email.Value, result.Email);
        Assert.Equal(customerEntity.DefaultAddress.Region, result.DefaultAddress.Region);
        Assert.Single(result.Addresses);
        Assert.Equal(customerEntity.Addresses.First().Region, result.Addresses.First().Region);
    }
}