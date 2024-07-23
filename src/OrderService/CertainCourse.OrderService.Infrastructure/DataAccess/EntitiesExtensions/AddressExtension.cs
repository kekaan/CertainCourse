using CustomersAddressProto = CertainCourse.Customers.Grpc.Address;
using AddressEntity = CertainCourse.OrderService.Domain.ValueObjects.Customer.Address;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

internal static class AddressExtension
{
    public static AddressEntity ToDomain(this CustomersAddressProto address)
    {
        return new AddressEntity
        {
            Region = address.Region,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            Apartment = address.Apartment,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
        };
    }
}