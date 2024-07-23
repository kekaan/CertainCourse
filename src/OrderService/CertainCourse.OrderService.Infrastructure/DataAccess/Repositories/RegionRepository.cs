using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;

internal sealed class RegionRepository : IRegionRepository
{
    private readonly IRegionDbRepository _regionDbRepository;
    private readonly IStorageDbRepository _storageDbRepository;

    public RegionRepository(IRegionDbRepository regionDbRepository, IStorageDbRepository storageDbRepository)
    {
        _regionDbRepository = regionDbRepository;
        _storageDbRepository = storageDbRepository;
    }

    public async Task<IReadOnlyCollection<RegionEntity>> GetRegionsAsync(CancellationToken cancellationToken)
    {
        var regions = await _regionDbRepository.GetAllAsync(cancellationToken);
        var storageIds = regions
            .Select(r => r.StorageId)
            .Distinct()
            .ToArray();
        
        var storages = await _storageDbRepository.GetByIds(storageIds, cancellationToken);

        return regions
            .Select(r => r.ToDomain(storages[r.StorageId]))
            .ToArray();
    }

    public Task CreateRegionAsync(RegionEntity region, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<RegionEntity> FindRegionByNameAsync(string regionName, CancellationToken cancellationToken)
    {
        var region = await _regionDbRepository.FindByNameAsync(regionName, cancellationToken);
        var storage = await _storageDbRepository.GetById(region.StorageId, cancellationToken);

        return region.ToDomain(storage);
    }

    public Task<bool> IsRegionExistAsync(int id, CancellationToken cancellationToken)
    {
        return _regionDbRepository.IsRegionExistAsync(id, cancellationToken);
    }
}