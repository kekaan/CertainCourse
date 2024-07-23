using FluentValidation;
using FluentValidation.Results;
using CertainCourse.GatewayService.Models.Orders.Dtos;

namespace CertainCourse.GatewayService.Models.Orders.Validation;

internal sealed class OrdersControllerValidator : IOrdersControllerValidator
{
    private readonly IValidator<GetOrdersAggregatedByRegionRequestDto> _getOrdersAggregatedByRegionRequestValidator;
    private readonly IValidator<GetOrdersByCustomerIdRequestDto> _getOrdersByCustomerIdRequestValidator;
    private readonly IValidator<GetOrdersRequestDto> _getOrdersRequestValidator;

    public OrdersControllerValidator(
        IValidator<GetOrdersAggregatedByRegionRequestDto> getOrdersAggregatedByRegionRequestValidator,
        IValidator<GetOrdersByCustomerIdRequestDto> getOrdersByCustomerIdRequestValidator,
        IValidator<GetOrdersRequestDto> getOrdersRequestValidator)
    {
        _getOrdersAggregatedByRegionRequestValidator = getOrdersAggregatedByRegionRequestValidator;
        _getOrdersByCustomerIdRequestValidator = getOrdersByCustomerIdRequestValidator;
        _getOrdersRequestValidator = getOrdersRequestValidator;
    }

    public ValidationResult ValidateGetOrdersAggregatedByRegionRequest(GetOrdersAggregatedByRegionRequestDto requestDto) =>
        _getOrdersAggregatedByRegionRequestValidator.Validate(requestDto);
    
    public ValidationResult ValidateGetOrdersByCustomerIdRequest(GetOrdersByCustomerIdRequestDto requestDto) =>
        _getOrdersByCustomerIdRequestValidator.Validate(requestDto);
    
    public ValidationResult ValidateGetOrdersRequest(GetOrdersRequestDto requestDto) =>
        _getOrdersRequestValidator.Validate(requestDto);
}