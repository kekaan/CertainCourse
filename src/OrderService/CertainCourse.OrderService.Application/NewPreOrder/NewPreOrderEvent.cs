namespace CertainCourse.OrderService.Application.NewPreOrder;

public sealed record NewPreOrderEvent(long Id, OrderSource Source, Customer Customer, IEnumerable<Good> Goods);