using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Models.Customers.Dtos;

namespace CertainCourse.GatewayService.Models.Customers;

public interface ICustomersService
{
    /// <summary>
    /// Получить список всех клиентов
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список всех клиентов</returns>
    Task<CallResult<CustomersDto>> GetCustomersAsync(CancellationToken cancellationToken);
}