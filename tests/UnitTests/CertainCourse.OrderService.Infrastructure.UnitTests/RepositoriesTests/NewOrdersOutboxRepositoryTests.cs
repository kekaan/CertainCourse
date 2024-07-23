using Moq;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;
using Xunit;

namespace CertainCourse.OrderService.Infrastructure.UnitTests.RepositoriesTests;

public class NewOrdersOutboxRepositoryTests
{
    private readonly Mock<INewOrdersOutboxDbRepository> _newOrdersOutboxDbRepositoryMock;
    private readonly NewOrdersOutboxRepository _newOrdersOutboxRepository;

    public NewOrdersOutboxRepositoryTests()
    {
        _newOrdersOutboxDbRepositoryMock = new Mock<INewOrdersOutboxDbRepository>();
        _newOrdersOutboxRepository = new NewOrdersOutboxRepository(_newOrdersOutboxDbRepositoryMock.Object);
    }

    [Fact]
    public async Task InsertAsync_ShouldCallInsertAsync()
    {
        // Arrange
        var message = new NewOrdersOutboxMessage(1, "NewOrder", "123", DateTime.Today, null);

        // Act
        await _newOrdersOutboxRepository.InsertAsync(message, CancellationToken.None);

        // Assert
        _newOrdersOutboxDbRepositoryMock.Verify(
            repo => repo.InsertAsync(
                It.IsAny<NewOrdersOutboxDal>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_ShouldReturnMessages()
    {
        // Arrange
        var messages = new List<NewOrdersOutboxDal> { new() };
        _newOrdersOutboxDbRepositoryMock.Setup(repo => repo.GetUnprocessedMessagesAsync(
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(messages);

        // Act
        var result =
            await _newOrdersOutboxRepository.GetUnprocessedMessagesAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldCallMarkAsProcessedAsync()
    {
        // Arrange
        var messageId = 1;

        // Act
        await _newOrdersOutboxRepository.MarkAsProcessedAsync(messageId, CancellationToken.None);

        // Assert
        _newOrdersOutboxDbRepositoryMock.Verify(
            repo => repo.MarkAsProcessedAsync(
                messageId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}