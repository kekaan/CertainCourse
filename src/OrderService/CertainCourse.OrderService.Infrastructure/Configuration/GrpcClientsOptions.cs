namespace CertainCourse.OrderService.Infrastructure.Configuration;

public sealed record GrpcClientsOptions
{
    public string ServiceDiscoveryClientAddress { get; set; } = null!;
    public string LogisticSimulatorClientAddress { get; set; } = null!;
    public string CustomerServiceClientAddress { get; set; } = null!;
}