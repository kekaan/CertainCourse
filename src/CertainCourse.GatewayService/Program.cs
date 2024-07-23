using Microsoft.AspNetCore.Server.Kestrel.Core;
using CertainCourse.GatewayService;
using CertainCourse.GatewayService.Infrastructure.Extensions;

const string CERTAIN_COURSE_HTTP_PORT = "CERTAIN_COURSE_HTTP_PORT";
 
await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(
        builder => builder.UseStartup<Startup>()
            .ConfigureKestrel(
                option =>
                {
                    option.ListenPortByOptions(CERTAIN_COURSE_HTTP_PORT, HttpProtocols.Http1);
                }))
    .Build()
    .RunAsync();