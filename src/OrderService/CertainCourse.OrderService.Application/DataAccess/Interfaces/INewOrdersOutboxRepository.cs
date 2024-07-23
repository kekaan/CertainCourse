using CertainCourse.OrderService.Application.DataAccess.Models;

namespace CertainCourse.OrderService.Application.DataAccess.Interfaces;

public interface INewOrdersOutboxRepository
{
    Task InsertAsync(NewOrdersOutboxMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NewOrdersOutboxMessage>> GetUnprocessedMessagesAsync(
        CancellationToken cancellationToken, string? type = null);
    Task MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken);
}