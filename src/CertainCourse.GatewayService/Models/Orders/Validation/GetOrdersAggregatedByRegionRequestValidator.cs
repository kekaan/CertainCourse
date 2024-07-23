using FluentValidation;
using CertainCourse.GatewayService.Models.Orders.Dtos;

namespace CertainCourse.GatewayService.Models.Orders.Validation;

internal sealed class GetOrdersAggregatedByRegionRequestValidator :
    AbstractValidator<GetOrdersAggregatedByRegionRequestDto>
{
    public GetOrdersAggregatedByRegionRequestValidator()
    {
        RuleFor(dto => dto.RegionsIds).NotEmpty().WithMessage("Region is not set");
    }
}