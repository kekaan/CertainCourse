using Moq;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.RepositoriesTests;

public class RegionRepositoryTests
{
    private readonly Mock<IRegionDbRepository> _regionDbRepositoryMock;
    private readonly Mock<IStorageDbRepository> _storageDbRepositoryMock;
    private readonly RegionRepository _regionRepository;

    public RegionRepositoryTests()
    {
        _regionDbRepositoryMock = new Mock<IRegionDbRepository>();
        _storageDbRepositoryMock = new Mock<IStorageDbRepository>();
        _regionRepository = new RegionRepository(_regionDbRepositoryMock.Object, _storageDbRepositoryMock.Object);
    }

    [Fact]
    public async Task GetRegionsAsync_ShouldReturnRegionsWithStorages()
    {
        // Arrange
        var regions = new List<RegionDal> { new RegionDal { Id = 1, StorageId = 1 } };
        var storages = new Dictionary<int, StorageDal> { { 1, new StorageDal { Latitude = 10, Longitude = 20 } } };

        _regionDbRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(regions);
        _storageDbRepositoryMock.Setup(repo => repo.GetByIds(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storages);

        // Act
        var result = await _regionRepository.GetRegionsAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task FindRegionByNameAsync_ShouldReturnRegionWithStorage()
    {
        // Arrange
        var regionName = "TestRegion";
        var region = new RegionDal { Id = 1, StorageId = 1 };
        var storage = new StorageDal { Latitude = 10, Longitude = 20 };

        _regionDbRepositoryMock.Setup(repo => repo.FindByNameAsync(regionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);
        _storageDbRepositoryMock.Setup(repo => repo.GetById(region.StorageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storage);

        // Act
        var result = await _regionRepository.FindRegionByNameAsync(regionName, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task IsRegionExistAsync_ShouldReturnTrueIfRegionExists()
    {
        // Arrange
        var regionId = 1;
        _regionDbRepositoryMock.Setup(repo => repo.IsRegionExistAsync(regionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _regionRepository.IsRegionExistAsync(regionId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }
}