using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using CertainCourse.OrderService;
using CertainCourse.OrderService.Infrastructure.Configuration;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.Sharded.Migration;
using CertainCourse.OrderService.Infrastructure.Extensions;

const string CERTAIN_COURSE_GRPC_PORT = "CERTAIN_COURSE_GRPC_PORT";

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(
        builder => builder.UseStartup<Startup>()
            .ConfigureKestrel(
                option =>
                {
                    option.ListenPortByOptions(CERTAIN_COURSE_GRPC_PORT, HttpProtocols.Http2);
                }))
    .Build()
    .RunOrMigrate();

namespace CertainCourse.OrderService
{
    internal static class ProgramExtension
    {
        public static async Task RunOrMigrate(this IHost host)
        {
            var scope = host.Services.CreateScope();
            var migrationSettings = scope.ServiceProvider.GetRequiredService<IOptions<MigrationOptions>>();

            if (!migrationSettings.Value.MigrateNeeded)
            {
                await host.RunAsync();
            }

            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var runner = scope.ServiceProvider.GetRequiredService<ShardMigrator>();
            await runner.Migrate(cts.Token);
        }
    }
}