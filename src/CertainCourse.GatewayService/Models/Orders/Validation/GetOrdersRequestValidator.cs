using FluentValidation;
using CertainCourse.GatewayService.Models.Orders.Dtos;

namespace CertainCourse.GatewayService.Models.Orders.Validation;

internal sealed class GetOrdersRequestValidator : AbstractValidator<GetOrdersRequestDto>
{
    private const string ORDER_BY_ASC = "asc";
    private const string ORDER_BY_DESC = "desc";
    private const int MIN_PAGE_SIZE = 0;
    private const int MIN_ID = 0;

    public GetOrdersRequestValidator()
    {
        RuleFor(dto => dto.RegionId)
            .NotEmpty().WithMessage("Region id was empty")
            .Must(id => id > MIN_ID).WithMessage("Incorrect region id");

        RuleFor(dto => dto.PageSize)
            .NotEmpty().WithMessage("Page size was empty")
            .Must(size => size > MIN_PAGE_SIZE).WithMessage($"Page size should be greater than {MIN_PAGE_SIZE}");

        RuleFor(dto => dto.OrderBy)
            .Must(order => order is ORDER_BY_ASC or ORDER_BY_DESC)
                .WithMessage($"OrderBy should be {ORDER_BY_ASC} or {ORDER_BY_DESC}");
    }
}