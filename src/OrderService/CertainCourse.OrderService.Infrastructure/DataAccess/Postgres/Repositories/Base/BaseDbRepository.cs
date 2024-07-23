using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;

namespace CertainCourse.OrderService.DataAccess.Postgres.Repositories.Base;

internal abstract class BaseDbRepository
{
    protected readonly IDatabaseConnectionFactory _connectionFactory;

    protected BaseDbRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
}