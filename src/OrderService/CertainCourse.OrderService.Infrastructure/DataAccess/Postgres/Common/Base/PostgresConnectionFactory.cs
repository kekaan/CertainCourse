using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;
using CertainCourse.OrderService.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.Infrastructure.Configuration;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Base
{
    internal sealed class PostgresConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly DatabaseOptions _databaseOptions;

        public PostgresConnectionFactory(IOptions<DatabaseOptions> databaseSettings)
        {
            _databaseOptions = databaseSettings.Value;
        }

        public IDbConnection GetConnection() => new NpgsqlConnection(_databaseOptions.ConnectionString);
    }
}
