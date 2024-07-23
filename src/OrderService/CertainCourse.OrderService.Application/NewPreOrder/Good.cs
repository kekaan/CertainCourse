namespace CertainCourse.OrderService.Application.NewPreOrder;

public sealed record Good(
    long Id,
    string Name,
    int Quantity,
    decimal Price,
    uint Weight);