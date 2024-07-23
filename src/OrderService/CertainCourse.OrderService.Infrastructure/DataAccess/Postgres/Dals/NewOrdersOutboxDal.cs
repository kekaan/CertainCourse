namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

internal record NewOrdersOutboxDal
{
    public long Id { get; init; }
    public string Type { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}