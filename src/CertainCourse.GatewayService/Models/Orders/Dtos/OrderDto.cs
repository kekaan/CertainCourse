using CertainCourse.GatewayService.Models.Common;
using CertainCourse.GatewayService.Models.Orders.Enums;

namespace CertainCourse.GatewayService.Models.Orders.Dtos;

public sealed record OrderDto(
    long Id,
    int GoodsCount,
    decimal TotalAmount,
    double TotalWeight,
    OrderState State,
    DateTime CreateDate,
    string Region,
    string CustomerFirstName,
    string CustomerLastName,
    AddressDto Address,
    string CustomerMobileNumber);