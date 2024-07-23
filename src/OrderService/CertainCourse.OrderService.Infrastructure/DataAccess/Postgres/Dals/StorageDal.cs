namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

internal sealed record StorageDal
{
    public int Id { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}