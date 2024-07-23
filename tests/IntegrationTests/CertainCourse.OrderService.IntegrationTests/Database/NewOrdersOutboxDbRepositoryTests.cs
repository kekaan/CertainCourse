using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.IntegrationTests.Database;

[Collection("Sequential")]
public class NewOrdersOutboxDbRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly NewOrdersOutboxDbRepository _newOrdersOutboxDbRepository;
    private readonly DatabaseFixture _fixture;

    public NewOrdersOutboxDbRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _newOrdersOutboxDbRepository = new NewOrdersOutboxDbRepository(_fixture.ConnectionFactory);
    }

    private async Task InsertTestOutboxMessagesAsync()
    {
        var outboxMessages = new List<NewOrdersOutboxDal>
        {
            new()
            {
                Id = 1, Type = "Type1", Payload = "Payload1", CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            },
            new()
            {
                Id = 2, Type = "Type2", Payload = "Payload2", CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            }
        };

        foreach (var message in outboxMessages)
        {
            await _newOrdersOutboxDbRepository.InsertAsync(message, CancellationToken.None);
        }
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_ShouldReturnUnprocessedMessages()
    {
        // Arrange
        await InsertTestOutboxMessagesAsync();
        
        // Act
        var unprocessedMessages = await _newOrdersOutboxDbRepository.GetUnprocessedMessagesAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(unprocessedMessages);
        Assert.Single(unprocessedMessages);
        Assert.Null(unprocessedMessages.First().ProcessedAt);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldMarkMessageAsProcessed()
    {
        // Arrange
        await InsertTestOutboxMessagesAsync();
        
        // Act
        var unprocessedMessages = await _newOrdersOutboxDbRepository.GetUnprocessedMessagesAsync(CancellationToken.None);
        var messageId = unprocessedMessages.First().Id;

        await _newOrdersOutboxDbRepository.MarkAsProcessedAsync(messageId, CancellationToken.None);
        var processedMessages = await _newOrdersOutboxDbRepository.GetUnprocessedMessagesAsync(CancellationToken.None);

        // Assert
        Assert.Empty(processedMessages);
    }

    public void Dispose() => _fixture.Dispose();
}