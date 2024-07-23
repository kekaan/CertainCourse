namespace CertainCourse.OrderService.Infrastructure.Configuration;

public sealed record MigrationOptions
{
    public bool MigrateNeeded { get; set; }
}