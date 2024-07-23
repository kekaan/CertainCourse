using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Region;

namespace CertainCourse.OrderService.Application.Validators.Orders;

internal interface INewOrderValidator
{
    bool IsValidByDistance(Address destinationAddress, Storage storage);
}