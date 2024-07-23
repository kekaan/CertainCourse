using CertainCourse.GatewayService.Models.Common;

namespace CertainCourse.GatewayService.Infrastructure.Extensions;

public static class AddressExtension
{
    public static AddressDto ToAddress(this Orders.Grpc.Address address)
    {
        return new AddressDto(
            address.Region,
            address.City,
            address.Street,
            address.Building,
            address.Apartment,
            address.Latitude,
            address.Longitude);
    }
    
    public static AddressDto ToAddress(this Customers.Grpc.Address address)
    {
        return new AddressDto(
            address.Region,
            address.City,
            address.Street,
            address.Building,
            address.Apartment,
            address.Latitude,
            address.Longitude);
    }
}