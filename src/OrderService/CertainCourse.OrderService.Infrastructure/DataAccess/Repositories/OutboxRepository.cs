using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Interfaces;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;

internal sealed class NewOrdersOutboxRepository : INewOrdersOutboxRepository
{
    private readonly INewOrdersOutboxDbRepository _newOrdersOutboxDbRepository;

    public NewOrdersOutboxRepository(INewOrdersOutboxDbRepository newOrdersOutboxDbRepository)
    {
        _newOrdersOutboxDbRepository = newOrdersOutboxDbRepository;
    }

    public async Task InsertAsync(NewOrdersOutboxMessage message, CancellationToken cancellationToken)
    {
        await _newOrdersOutboxDbRepository.InsertAsync(message.ToDal(), cancellationToken);
    }

    public async Task<IReadOnlyCollection<NewOrdersOutboxMessage>> GetUnprocessedMessagesAsync(
        CancellationToken cancellationToken,
        string? type = null)
    {
        var messages =
            await _newOrdersOutboxDbRepository.GetUnprocessedMessagesAsync(cancellationToken, type);

        return messages.Select(m => m.ToEntity()).ToArray();
    }

    public async Task MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken)
    {
        await _newOrdersOutboxDbRepository.MarkAsProcessedAsync(messageId, cancellationToken);
    }
}