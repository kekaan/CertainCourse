namespace CertainCourse.OrderService.Application.NewPreOrder;

public sealed record Address(
    string Region,
    string City,
    string Street,
    string Building,
    string Apartment,
    double Latitude,
    double Longitude);