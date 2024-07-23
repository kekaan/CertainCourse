using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;

namespace CertainCourse.OrderService.IntegrationTests.Database;

[Collection("Sequential")]
public class RegionDbRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly RegionDbRepository _regionDbRepository;
    private readonly DatabaseFixture _fixture;

    public RegionDbRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _regionDbRepository = new RegionDbRepository(_fixture.ConnectionFactory);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRegion_WhenIdExists()
    {
        var region = await _regionDbRepository.GetByIdAsync(1, CancellationToken.None);

        Assert.NotNull(region);
        Assert.Equal(1, region.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnRegions_WhenIdsExist()
    {
        var regionIds = new List<int> { 1, 2 };
        var regions = await _regionDbRepository.GetByIdsAsync(regionIds, CancellationToken.None);

        Assert.NotNull(regions);
        Assert.Equal(2, regions.Count);
        Assert.Contains(regions, r => r.Key == 1);
        Assert.Contains(regions, r => r.Key == 2);
    }

    public void Dispose() => _fixture.Dispose();
}