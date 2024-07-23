namespace CertainCourse.OrderService.Common.RequestsSpecifications;

public sealed record PaginationSpecification(int PageSize, string PageToken);