using CertainCourse.OrderService.Domain.ValueObjects.Customer;

namespace CertainCourse.OrderService.Domain;

public sealed record CustomerEntity(
    CustomerId Id,
    string FirstName,
    string LastName,
    MobileNumber MobileNumber,
    Email Email,
    Address DefaultAddress,
    Address[] Addresses);