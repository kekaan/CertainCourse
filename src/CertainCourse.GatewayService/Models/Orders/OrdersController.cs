using Microsoft.AspNetCore.Mvc;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Infrastructure.Validation;
using CertainCourse.GatewayService.Models.Orders.Dtos;
using CertainCourse.GatewayService.Models.Orders.Validation;

namespace CertainCourse.GatewayService.Models.Orders;

[ApiController]
[Route("[controller]")]
public class OrdersController : Controller
{
    private readonly IOrdersService _ordersService;
    private readonly IOrdersControllerValidator _controllerValidator;

    public OrdersController(IOrdersService ordersService, IOrdersControllerValidator controllerValidator)
    {
        _ordersService = ordersService;
        _controllerValidator = controllerValidator;
    }

    /// <summary>
    /// Отменить заказ по Id заказа
    /// </summary>
    /// <param name="orderId">Id ордера</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    [HttpPost("CancelById/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(CallResult), 400)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(CallResult), 500)]
    public async Task<IActionResult> CancelOrderById(long orderId, CancellationToken cancellationToken)
    {
        if (orderId < 0)
            return BadRequest("Incorrect Id");

        var callResult = await _ordersService.CancelOrderByIdAsync(orderId, cancellationToken: cancellationToken);

        if (!callResult.Success)
        {
            if (callResult.StatusCode is CallResultStatusCode.NotFound)
                return NotFound(callResult.ErrorMessage);

            if (callResult.StatusCode is CallResultStatusCode.FailedPrecondition)
                return BadRequest(callResult);

            return StatusCode(StatusCodes.Status500InternalServerError, callResult.ErrorMessage);
        }
        
        return Ok();
    }

    /// <summary>
    /// Получить статус заказа по Id заказа
    /// </summary>
    /// <param name="orderId">Id ордера</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    [HttpGet("GetStateById/{orderId}")]
    [ProducesResponseType(typeof(OrderStateDto), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(CallResult), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetOrderStateById(long orderId, CancellationToken cancellationToken)
    {
        if (orderId < 0)
            return BadRequest("Incorrect Id");

        CallResult<OrderStateDto> callResult = await _ordersService.GetOrderStateByIdAsync(orderId, cancellationToken: cancellationToken);

        if (!callResult.Success)
        {
            if (callResult.StatusCode is CallResultStatusCode.NotFound)
                return NotFound(callResult.ErrorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError, callResult.ErrorMessage);
        }

        return Json(callResult.Data);
    }

    /// <summary>
    /// Получить список регионов
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("GetRegions")]
    [ProducesResponseType(typeof(RegionsDto), 200)]
    [ProducesResponseType(typeof(CallResult), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetRegions(CancellationToken cancellationToken)
    {
        var callResult = await _ordersService.GetRegionsAsync(cancellationToken: cancellationToken);

        if (!callResult.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, callResult.ErrorMessage);
        }

        return Json(callResult.Data);
    }

    /// <summary>
    /// Получить список заказов
    /// </summary>
    /// <param name="requestDto">Запрос</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("Get")]
    [ProducesResponseType(typeof(GetOrdersResponseDto), 200)]
    [ProducesResponseType(typeof(List<ValidationFailureDto>), 400)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(CallResult), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = _controllerValidator.ValidateGetOrdersRequest(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors
                .Select(e => new ValidationFailureDto(e.PropertyName, e.ErrorMessage)));
        }

        var callResult = await _ordersService.GetOrdersAsync(requestDto, cancellationToken: cancellationToken);

        if (!callResult.Success)
        {
            if (callResult.StatusCode is CallResultStatusCode.NotFound)
                return BadRequest(callResult.ErrorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError, callResult);
        }

        return Json(callResult.Data);
    }

    /// <summary>
    /// Получить список агрегации заказов по региону
    /// </summary>
    /// <param name="requestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("GetAggregatedByRegion")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrdersAggregatedByRegionDto>), 200)]
    [ProducesResponseType(typeof(List<ValidationFailureDto>), 400)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(CallResult), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetOrdersAggregatedByRegion(
        [FromQuery] GetOrdersAggregatedByRegionRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = _controllerValidator.ValidateGetOrdersAggregatedByRegionRequest(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors
                .Select(e => new ValidationFailureDto(e.PropertyName, e.ErrorMessage)));
        }

        var callResult =
            await _ordersService.GetOrdersAggregatedByRegionAsync(requestDto, cancellationToken: cancellationToken);

        if (!callResult.Success)
        {
            if (callResult.StatusCode is CallResultStatusCode.NotFound)
                return BadRequest(callResult.ErrorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError, callResult);
        }

        return Json(callResult.Data);
    }

    /// <summary>
    /// Получить список заказов по Id клиента
    /// </summary>
    /// <param name="requestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("GetByCustomerId")]
    [ProducesResponseType(typeof(GetOrdersByCustomerIdResponseDto), 200)]
    [ProducesResponseType(typeof(List<ValidationFailureDto>), 400)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(CallResult), 500)]
    [Produces("application/json")]
    public async Task<IActionResult> GetOrdersByCustomerId(
        [FromQuery] GetOrdersByCustomerIdRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = _controllerValidator.ValidateGetOrdersByCustomerIdRequest(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors
                .Select(e => new ValidationFailureDto(e.PropertyName, e.ErrorMessage)));
        }

        var callResult =
            await _ordersService.GetOrdersByCustomerIdAsync(requestDto, cancellationToken: cancellationToken);

        if (!callResult.Success)
        {
            if (callResult.StatusCode is CallResultStatusCode.NotFound)
                return NotFound(callResult.ErrorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError, callResult);
        }

        return Json(callResult.Data);
    }
}