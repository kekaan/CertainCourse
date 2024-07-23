namespace CertainCourse.OrderService.Application.DataAccess.Models;

public record NewOrdersOutboxMessage(
    long Id,
    string Type,
    string Payload,
    DateTime CreatedAt,
    DateTime? ProcessedAt);