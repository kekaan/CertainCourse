using AddressEntity = CertainCourse.OrderService.Domain.ValueObjects.Customer.Address;
using MessageBrokerAddress = CertainCourse.OrderService.Application.NewPreOrder.Address;

namespace CertainCourse.OrderService.Application.Extensions;

internal static class AddressExtension
{
    public static AddressEntity ToDomain(this MessageBrokerAddress address)
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