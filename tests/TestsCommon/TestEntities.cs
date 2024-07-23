using CertainCourse.OrderService.Application.NewPreOrder;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using DomainAddress = CertainCourse.OrderService.Domain.ValueObjects.Customer.Address;
using PreOrderAddress = CertainCourse.OrderService.Application.NewPreOrder.Address;
using PreOrderCustomer = CertainCourse.OrderService.Application.NewPreOrder.Customer;

namespace TestsCommon;

public static class TestEntities
{
    public static OrderEntity Order { get; } = new(
        Id: new OrderId(12345),
        GoodsCount: 3,
        TotalAmount: 150.75m,
        TotalWeight: 2.5,
        Type: OrderType.Web,
        CreateDate: DateTime.UtcNow,
        RegionId: new RegionId(1),
        RegionName: "Moscow",
        State: OrderState.Created,
        CustomerId: new CustomerId(1),
        CustomerFirstName: "Vasya",
        CustomerLastName: "Pupkin",
        CustomerMobileNumber: MobileNumber.Create("+1234567890"),
        CustomerAddress: new DomainAddress
        {
            Region = "Moscow",
            City = "Moscow",
            Street = "Tverskaya",
            Building = "12",
            Apartment = "34",
            Latitude = 55.7558,
            Longitude = 37.6173
        }
    );

    public static CustomerEntity Customer { get; } = new(
        Id: new CustomerId(1),
        FirstName: "Vasya",
        LastName: "Pupkin",
        MobileNumber: MobileNumber.Create("+1234567890"),
        Email: Email.Create("vasya.Pupkin@mail.ru"),
        DefaultAddress: new DomainAddress
        {
            Region = "Moscow",
            City = "Moscow",
            Street = "St",
            Building = "123",
            Apartment = "45",
            Latitude = 37.7749,
            Longitude = -122.4194
        },
        Addresses:
        [
            new DomainAddress
            {
                Region = "Central",
                City = "Springfield",
                Street = "Main St",
                Building = "123A",
                Apartment = "45",
                Latitude = 37.7749,
                Longitude = -122.4194
            },
            new DomainAddress
            {
                Region = "Westside",
                City = "Shelbyville",
                Street = "Oak St",
                Building = "456B",
                Apartment = "67",
                Latitude = 34.0522,
                Longitude = -118.2437
            }
        ]
    );

    public static NewPreOrderEvent PreOrder { get; } = new(
        Id: 1,
        Source: OrderSource.Api,
        Customer: new PreOrderCustomer
        (
            1,
            new PreOrderAddress(
                "TestRegion",
                "TestCity",
                "TestStreet",
                "123",
                "456",
                0.0,
                0.0
            )
        ),
        Goods: new List<Good>
        {
            new Good(1, "Item1", 1, 10.0m, 2),
            new Good(2, "Item2", 1, 10.0m, 2)
        });
}