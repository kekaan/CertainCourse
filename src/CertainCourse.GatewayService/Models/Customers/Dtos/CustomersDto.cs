namespace CertainCourse.GatewayService.Models.Customers.Dtos;

public sealed record CustomersDto(IReadOnlyCollection<CustomerDto> Customers);