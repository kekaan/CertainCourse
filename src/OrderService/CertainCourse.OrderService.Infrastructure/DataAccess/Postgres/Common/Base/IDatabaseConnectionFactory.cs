using System.Data;

namespace CertainCourse.OrderService.DataAccess.Postgres.Common.Base
{
    public interface IDatabaseConnectionFactory
    {
        public IDbConnection GetConnection();
    }
}
