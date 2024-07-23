namespace CertainCourse.OrderService.Domain.ValueObjects.Customer;

public readonly record struct CustomerId(int Value)
{
    public static implicit operator int(CustomerId customerId) => customerId.Value;
}