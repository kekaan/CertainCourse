namespace CertainCourse.OrderService.Infrastructure.Configuration;

public sealed record DatabaseOptions
{
    public string ConnectionString { get; set; } = null!;
}