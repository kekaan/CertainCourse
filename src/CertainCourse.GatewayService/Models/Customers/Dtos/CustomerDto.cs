using CertainCourse.GatewayService.Models.Common;

namespace CertainCourse.GatewayService.Models.Customers.Dtos;

public sealed record CustomerDto(
    long Id,
    string FirstName,
    string LastName,
    string MobileNumber,
    string Email,
    AddressDto DefaultAddress,
    IReadOnlyCollection<AddressDto> Addressed);