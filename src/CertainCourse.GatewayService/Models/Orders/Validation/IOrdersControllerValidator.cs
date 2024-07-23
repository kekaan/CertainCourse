using FluentValidation.Results;
using CertainCourse.GatewayService.Models.Orders.Dtos;

namespace CertainCourse.GatewayService.Models.Orders.Validation;

public interface IOrdersControllerValidator
{
    public ValidationResult ValidateGetOrdersAggregatedByRegionRequest(
        GetOrdersAggregatedByRegionRequestDto requestDto);

    public ValidationResult ValidateGetOrdersByCustomerIdRequest(GetOrdersByCustomerIdRequestDto requestDto);
    public ValidationResult ValidateGetOrdersRequest(GetOrdersRequestDto requestDto);
}