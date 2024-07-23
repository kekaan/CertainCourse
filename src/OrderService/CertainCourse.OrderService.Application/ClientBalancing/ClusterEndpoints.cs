namespace CertainCourse.OrderService.Application.ClientBalancing;

public sealed record ClusterEndpoints(DateTime LastUpdated, DbEndpoint[] Endpoints);