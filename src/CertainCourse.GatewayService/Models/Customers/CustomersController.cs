using Microsoft.AspNetCore.Mvc;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Models.Customers.Dtos;

namespace CertainCourse.GatewayService.Models.Customers;

[ApiController]
[Route("[controller]")]
public class CustomersController : Controller
{
    private readonly ICustomersService _customersService;

    public CustomersController(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    /// <summary>
    /// Получить список всех клиентов
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    [HttpGet("Get")]
    [ProducesResponseType(typeof(CustomersDto), 200)]
    [ProducesResponseType(typeof(CallResult), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var callResult = await _customersService.GetCustomersAsync(cancellationToken);

        if (!callResult.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, callResult);
        }

        return Json(callResult.Data);
    }
}