using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Base;

namespace CertainCourse.OrderService.IntegrationTests.Database;

[Collection("Sequential")]
public class StorageDbRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly StorageDbRepository _storageDbRepository;
    private readonly DatabaseFixture _fixture;

    public StorageDbRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _storageDbRepository = new StorageDbRepository(_fixture.ConnectionFactory);
    }

    [Fact]
    public async Task GetById_ShouldReturnStorage_WhenIdExists()
    {
        var storage = await _storageDbRepository.GetById(1, CancellationToken.None);

        Assert.NotNull(storage);
        Assert.Equal(1, storage.Id);
    }

    [Fact]
    public async Task GetByIds_ShouldReturnStorages_WhenIdsExist()
    {
        var storageIds = new List<int> { 1, 2 };
        var storages = await _storageDbRepository.GetByIds(storageIds, CancellationToken.None);

        Assert.NotNull(storages);
        Assert.Equal(2, storages.Count);
        Assert.Contains(storages, s => s.Key == 1);
        Assert.Contains(storages, s => s.Key == 2);
    }

    public void Dispose() => _fixture.Dispose();
}