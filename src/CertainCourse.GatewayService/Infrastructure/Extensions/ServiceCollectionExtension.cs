using System.Reflection;
using FluentValidation;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using Microsoft.OpenApi.Models;
using CertainCourse.GatewayService.Models.Customers;
using CertainCourse.GatewayService.Models.Orders;
using CertainCourse.GatewayService.Models.Orders.Dtos;
using CertainCourse.GatewayService.Models.Orders.Validation;

namespace CertainCourse.GatewayService.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddScoped<LoggingInterceptor>();
        
        serviceCollection.AddOrdersServiceGrpcClient(configuration);
        serviceCollection.AddCustomersServiceGrpcClient(configuration);

        return serviceCollection;
    }

    private static IServiceCollection AddOrdersServiceGrpcClient(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddSingleton<ResolverFactory>(
            _ =>
            {
                var url1 = configuration.GetValue<string>("CERTAIN_COURSE_ORDER_SERVICE_1_ADDRESS");
                var url2 = configuration.GetValue<string>("CERTAIN_COURSE_ORDER_SERVICE_2_ADDRESS");

                if (string.IsNullOrEmpty(url1))
                {
                    throw new ArgumentException(
                        "Требуется указать переменную окружения CERTAIN_COURSE_ORDERS_SERVICE_1_ADDRESS или она пустая");
                }

                if (string.IsNullOrEmpty(url2))
                {
                    throw new ArgumentException(
                        "Требуется указать переменную окружения CERTAIN_COURSE_ORDERS_SERVICE_2_ADDRESS или она пустая");
                }

                Uri uri1 = new Uri(url1);
                Uri uri2 = new Uri(url2);
                return new StaticResolverFactory(
                    _ =>
                    [
                        new BalancerAddress(uri1.Host, uri1.Port),
                        new BalancerAddress(uri2.Host, uri2.Port)
                    ]);
            });

        serviceCollection.AddGrpcClient<CertainCourse.Orders.Grpc.Orders.OrdersClient>(
                options =>
                {
                    options.Address = new Uri("static://order-address");
                    options.InterceptorRegistrations.Add(new InterceptorRegistration(
                        InterceptorScope.Channel, 
                        provider => provider.GetRequiredService<LoggingInterceptor>()));
                })
            .ConfigureChannel(
                options =>
                {
                    options.Credentials = ChannelCredentials.Insecure;
                    options.ServiceConfig = new ServiceConfig
                    {
                        LoadBalancingConfigs =
                        {
                            new RoundRobinConfig()
                        }
                    };
                });

        return serviceCollection;
    }

    private static IServiceCollection AddCustomersServiceGrpcClient(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddGrpcClient<CertainCourse.Customers.Grpc.Customers.CustomersClient>(
            options =>
            {
                var url = configuration.GetValue<string>("CERTAIN_COURSE_CUSTOMER_SERVICE_ADDRESS");

                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException(
                        "Требуется указать переменную окружения CERTAIN_COURSE_SD_ADDRESS или она пустая");
                }

                options.Address = new Uri(url);
            });

        return serviceCollection;
    }

    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICustomersService, GrpcCustomersService>();
        serviceCollection.AddScoped<IOrdersService, GrpcOrdersService>();

        return serviceCollection;
    }

    public static IServiceCollection AddControllersValidation(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<IValidator<GetOrdersAggregatedByRegionRequestDto>,
                GetOrdersAggregatedByRegionRequestValidator>();
        serviceCollection
            .AddScoped<IValidator<GetOrdersByCustomerIdRequestDto>, GetOrdersByCustomerIdRequestValidator>();
        serviceCollection.AddScoped<IValidator<GetOrdersRequestDto>, GetOrdersRequestValidator>();
        serviceCollection.AddScoped<IOrdersControllerValidator, OrdersControllerValidator>();

        return serviceCollection;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway Service", Version = "v1" });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return serviceCollection;
    }
}