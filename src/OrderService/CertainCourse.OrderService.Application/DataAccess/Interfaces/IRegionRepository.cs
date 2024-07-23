using CertainCourse.OrderService.Domain;

namespace CertainCourse.OrderService.Application.DataAccess.Interfaces;

public interface IRegionRepository
{
    Task<IReadOnlyCollection<RegionEntity>> GetRegionsAsync(CancellationToken cancellationToken);
    Task CreateRegionAsync(RegionEntity region, CancellationToken cancellationToken);
    public Task<RegionEntity> FindRegionByNameAsync(string regionName, CancellationToken cancellationToken);
    Task<bool> IsRegionExistAsync(int id, CancellationToken cancellationToken);
}