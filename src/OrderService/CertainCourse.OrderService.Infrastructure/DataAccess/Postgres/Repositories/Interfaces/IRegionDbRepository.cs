using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;

internal interface IRegionDbRepository
{
    Task<RegionDal> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, RegionDal>> GetByIdsAsync(IReadOnlyCollection<int> regionIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RegionDal>> GetAllAsync(CancellationToken cancellationToken);

    Task<RegionDal> FindByNameAsync(string regionName, CancellationToken cancellationToken);
    
    Task<bool> IsRegionExistAsync(int id, CancellationToken cancellationToken);
}