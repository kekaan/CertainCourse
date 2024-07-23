using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Models.Orders.Dtos;

namespace CertainCourse.GatewayService.Models.Orders;

public interface IOrdersService
{
    /// <summary>
    /// Отменить заказ по его Id
    /// </summary>
    /// <param name="orderId">Id заказа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<CallResult<CancelOrderByIdResponseDto>> CancelOrderByIdAsync(long orderId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить статус заказа по его Id
    /// </summary>
    /// <param name="orderId">Id заказа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<CallResult<OrderStateDto>> GetOrderStateByIdAsync(long orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить список всех регионов
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<CallResult<RegionsDto>> GetRegionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить все ордера исходя из заданных параметров.
    /// </summary>
    /// <param name="requestDto">Включает в себя поле фильтрации (регион), размер страницы,
    /// токен страницы и параметр сортировки (asc/desc)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<CallResult<GetOrdersResponseDto>> GetOrdersAsync(GetOrdersRequestDto requestDto,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить агрегат заказов для определенного региона
    /// </summary>
    /// <param name="requestDto">Включает в себя регион, время для фильтрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<CallResult<IReadOnlyCollection<OrdersAggregatedByRegionDto>>> GetOrdersAggregatedByRegionAsync(
        GetOrdersAggregatedByRegionRequestDto requestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Получить список заказов по Id клиента
    /// </summary>
    /// <param name="requestDto">Включает в себя Id клиента и
    /// параметры фильтрации (дата/время, размер страницы и токен страницы)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<CallResult<GetOrdersByCustomerIdResponseDto>> GetOrdersByCustomerIdAsync(
        GetOrdersByCustomerIdRequestDto requestDto, CancellationToken cancellationToken);
}