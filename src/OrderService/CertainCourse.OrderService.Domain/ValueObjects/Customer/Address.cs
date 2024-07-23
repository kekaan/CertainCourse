namespace CertainCourse.OrderService.Domain.ValueObjects.Customer;

public sealed record Address
{
    public string Region { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string Building { get; init; } = null!;
    public string Apartment { get; init; } = null!;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}