using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;

internal interface IStorageDbRepository
{
    Task<StorageDal> GetById(int id, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, StorageDal>> GetByIds(IReadOnlyCollection<int> storageIds,
        CancellationToken cancellationToken);
}