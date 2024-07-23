using CertainCourse.OrderService.Domain;

namespace CertainCourse.OrderService.Application.DataAccess.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerEntity> GetCustomerByIdAsync(int id, CancellationToken cancellationToken);
}