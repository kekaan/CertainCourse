using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CertainCourse.OrderService.IntegrationTests;

public sealed class TestApplication : WebApplicationFactory<Startup>
{
    protected override IHostBuilder? CreateHostBuilder()
    {
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_LOGISTIC_SIMULATOR_ADDRESS", "http://logistic-simulator:8080");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_SD_ADDRESS", "http://localhost:5500");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_GRPC_PORT", "8080");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_KAFKA_BROKERS", "localhost:29091");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_KAFKA_PRE_ORDER_TOPIC", "pre_orders");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_KAFKA_NEW_ORDER_TOPIC", "new_orders");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_KAFKA_ORDER_EVENTS_TOPIC", "orders_events");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_REDIS_CONNECT", "localhost:6379");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_REDIS_DATABASE", "0");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_CUSTOMER_ADDRESS", "http://localhost:5081");
        Environment.SetEnvironmentVariable("CERTAIN_COURSE_DATABASE_MIGRATE_NEEDED", "false");

        return base.CreateHostBuilder();
    }
}