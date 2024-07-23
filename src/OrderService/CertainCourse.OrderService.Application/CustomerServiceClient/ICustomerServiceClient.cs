using CertainCourse.OrderService.Domain;

namespace CertainCourse.OrderService.Application.CustomerServiceClient;

public interface ICustomerServiceClient
{
    Task<CustomerEntity> GetCustomer(int id, CancellationToken cancellationToken);
}