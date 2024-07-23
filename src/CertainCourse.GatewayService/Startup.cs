using CertainCourse.GatewayService.Infrastructure.Extensions;

namespace CertainCourse.GatewayService;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddLogging(e => e.AddConsole());
        serviceCollection.AddGrpcClients(_configuration);
        
        serviceCollection.AddServices();
        
        serviceCollection.AddControllersValidation();
        serviceCollection.AddControllers();
        
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwagger();
    }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseRouting();
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        });
        
        applicationBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}