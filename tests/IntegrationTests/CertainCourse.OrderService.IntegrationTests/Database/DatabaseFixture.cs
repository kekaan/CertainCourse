using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Base;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.TypeHandlers;

namespace CertainCourse.OrderService.IntegrationTests.Database;

public class DatabaseFixture : IDisposable
{
    public IDbConnection Connection { get; }
    internal PostgresConnectionFactory ConnectionFactory { get; }

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        var databaseSettings = Options.Create(new DatabaseOptions()
        {
            ConnectionString = configuration.GetConnectionString("OrderServiceDatabase")
        });

        SqlMapper.AddTypeHandler(new JsonTypeHandler<Address>());
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        ConnectionFactory = new PostgresConnectionFactory(databaseSettings);
        Connection = ConnectionFactory.GetConnection();
        
        ClearDatabase();
    }

    public void Dispose()
    {
        ClearDatabase();
    }
    
    public void ClearDatabase()
    {
        Connection.Execute("DELETE FROM customer_infos;");
        Connection.Execute("DELETE FROM orders;");
        Connection.Execute("DELETE FROM outbox_messages;");
    }
}