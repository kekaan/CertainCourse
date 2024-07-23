using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.DataAccess.Postgres.Repositories.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Interfaces;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Sharded;
using CertainCourse.OrderService.Infrastructure.DataAccess.Repositories;

namespace CertainCourse.OrderService.Infrastructure.DataAccess;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRepository(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IOrderDbRepository, OrderShardedDbRepository>();
        serviceCollection.AddScoped<INewOrdersOutboxDbRepository, NewOrdersOutboxShardedDbRepository>();
        serviceCollection.AddScoped<IRegionDbRepository, RegionShardedDbRepository>();
        serviceCollection.AddScoped<IStorageDbRepository, StorageShardedDbRepository>();

        serviceCollection.AddScoped<IOrderRepository, OrderRepository>();
        serviceCollection.AddScoped<IRegionRepository, RegionRepository>();
        serviceCollection.AddScoped<INewOrdersOutboxRepository, NewOrdersOutboxRepository>();
        serviceCollection.AddScoped<ICustomerRepository, CustomerRepository>();

        return serviceCollection;
    }
}