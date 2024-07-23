namespace CertainCourse.OrderService.Application.ClientBalancing;

public sealed record DbEndpoint(string HostAndPort, DbReplicaType DbReplica, int[] Buckets);