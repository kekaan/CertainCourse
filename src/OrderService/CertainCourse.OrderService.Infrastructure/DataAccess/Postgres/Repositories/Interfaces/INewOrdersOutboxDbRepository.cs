using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Interfaces;

internal interface INewOrdersOutboxDbRepository
{
    Task InsertAsync(NewOrdersOutboxDal newOrdersOutboxMessage, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NewOrdersOutboxDal>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken,
        string? type = null);

    Task MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken);
}