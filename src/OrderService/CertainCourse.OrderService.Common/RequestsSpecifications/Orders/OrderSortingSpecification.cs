namespace CertainCourse.OrderService.Common.RequestsSpecifications.Orders;

public sealed record OrderSortingSpecification(OrderProperty Property, bool SortDescending);