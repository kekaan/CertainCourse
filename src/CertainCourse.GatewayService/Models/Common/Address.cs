namespace CertainCourse.GatewayService.Models.Common;

public sealed record AddressDto(
    string Region,
    string City,
    string Street,
    string Building,
    string Apartment,
    double Latitude,
    double Longitude);