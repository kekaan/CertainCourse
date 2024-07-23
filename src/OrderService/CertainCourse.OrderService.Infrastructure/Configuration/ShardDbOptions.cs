namespace CertainCourse.OrderService.Infrastructure.Configuration;

public sealed class ShardDbOptions
{
    public string ClusterName { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
}