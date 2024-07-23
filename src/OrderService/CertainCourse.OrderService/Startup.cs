using CertainCourse.OrderService.Application;
using CertainCourse.OrderService.BackgroundJobs;
using CertainCourse.OrderService.Infrastructure;
using CertainCourse.OrderService.Infrastructure.Grpc;
using CertainCourse.OrderService.Infrastructure.Grpc.Services;

namespace CertainCourse.OrderService;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddBackgroundJobs()
            .AddGrpcServers()
            .AddInfrastructure(_configuration)
            .AddApplication()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseRouting();
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();

        applicationBuilder.UseEndpoints(
            endpointRouteBuilder =>
            {
                endpointRouteBuilder.MapGrpcReflectionService();
                endpointRouteBuilder.MapGrpcService<OrdersService>();
            });
    }
}