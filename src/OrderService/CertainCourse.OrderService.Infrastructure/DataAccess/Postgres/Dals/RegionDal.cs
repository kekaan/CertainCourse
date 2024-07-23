namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

internal sealed class RegionDal()
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int StorageId { get; init; }
}