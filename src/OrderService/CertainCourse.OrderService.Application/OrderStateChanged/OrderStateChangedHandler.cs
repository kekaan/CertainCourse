using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.Extensions;
using CertainCourse.OrderService.Application.MessageBrokers;

namespace CertainCourse.OrderService.Application.OrderStateChanged;

internal sealed class OrderStateChangedHandler : IOrderStateChangedHandler
{
    private readonly IOrderRepository _orderRepository;

    public OrderStateChangedHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<HandlerResult> Handle(OrderStateChangedEvent preOrderRequest, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(preOrderRequest.OrderId, cancellationToken);

            var updatedOrder = order with { State = preOrderRequest.OrderState.ToDomain() };
            await _orderRepository.UpdateOrderAsync(updatedOrder, cancellationToken);

            return HandlerResult.Ok;
        }
        catch (Exception exception)
        {
            throw new OrderStateChangedException(exception.Message);
        }
    }
}