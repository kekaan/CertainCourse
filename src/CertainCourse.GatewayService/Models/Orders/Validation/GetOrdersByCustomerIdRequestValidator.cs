using FluentValidation;
using CertainCourse.GatewayService.Models.Orders.Dtos;

namespace CertainCourse.GatewayService.Models.Orders.Validation;

internal sealed class GetOrdersByCustomerIdRequestValidator : AbstractValidator<GetOrdersByCustomerIdRequestDto>
{
    private const int MIN_ID = 0;
    private const int MIN_PAGE_SIZE = 0;
    
    public GetOrdersByCustomerIdRequestValidator()
    {
        RuleFor(dto => dto.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id was empty")
            .Must(id => id > MIN_ID)
            .WithMessage("Incorrect customer id");
        
        RuleFor(dto => dto.PageSize)
            .NotEmpty()
            .WithMessage("Page size was empty")
            .Must(size => size > MIN_PAGE_SIZE).WithMessage($"Page size should be greater than {MIN_PAGE_SIZE}");
    }
}