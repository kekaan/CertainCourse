using Microsoft.Extensions.DependencyInjection;
using CertainCourse.OrderService.Application.NewPreOrder;
using CertainCourse.OrderService.Application.OrderStateChanged;
using CertainCourse.OrderService.Application.Validators.Orders;

namespace CertainCourse.OrderService.Application;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddHandlers()
            .AddValidators();
    }

    private static IServiceCollection AddHandlers(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<INewPreOrderHandler, NewPreOrderHandler>()
            .AddScoped<IOrderStateChangedHandler, OrderStateChangedHandler>();
    }
    
    private static IServiceCollection AddValidators(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<INewOrderValidator, NewOrderValidator>();
        
        return serviceCollection;
    }
}