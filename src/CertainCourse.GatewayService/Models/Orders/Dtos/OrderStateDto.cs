using CertainCourse.GatewayService.Infrastructure.Extensions;
using CertainCourse.GatewayService.Models.Orders.Enums;

namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record OrderStateDto()
{
    public OrderState OrderState { get; init; }

    public OrderStateDto(CertainCourse.Orders.Grpc.OrderState orderState) : this()
    {
        OrderState = orderState.ToOrderState();
    }
}