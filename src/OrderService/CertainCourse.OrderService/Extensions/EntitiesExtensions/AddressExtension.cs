using OrdersGrpcAddress = CertainCourse.Orders.Grpc.Address;
using AddressEntity = CertainCourse.OrderService.Domain.ValueObjects.Customer.Address;

namespace CertainCourse.OrderService.Extensions.EntitiesExtensions;

internal static class AddressExtension
{
    public static OrdersGrpcAddress ToProto(this AddressEntity address)
    {
        return new OrdersGrpcAddress
        {
            Apartment = address.Apartment,
            Building = address.Building,
            City = address.City,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            Region = address.Region,
            Street = address.Street
        };
    }
}