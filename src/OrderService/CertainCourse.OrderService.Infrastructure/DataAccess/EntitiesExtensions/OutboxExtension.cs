using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;

internal static class OutboxExtension
{
    public static NewOrdersOutboxDal ToDal(this NewOrdersOutboxMessage message)
    {
        return new NewOrdersOutboxDal
        {
            Id = message.Id,
            Type = message.Type,
            Payload = message.Payload,
            CreatedAt = message.CreatedAt,
            ProcessedAt = message.ProcessedAt
        };
    }

    public static NewOrdersOutboxMessage ToEntity(this NewOrdersOutboxDal dal)
    {
        return new NewOrdersOutboxMessage(
            Id: dal.Id,
            Type: dal.Type,
            Payload: dal.Payload,
            CreatedAt: dal.CreatedAt,
            ProcessedAt: dal.ProcessedAt);
    }
}