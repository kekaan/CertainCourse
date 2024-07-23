using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.Extensions;
using CertainCourse.OrderService.Application.MessageBrokers;
using CertainCourse.OrderService.Application.Validators.Orders;
using CertainCourse.OrderService.Domain;

namespace CertainCourse.OrderService.Application.NewPreOrder;

internal sealed class NewPreOrderHandler : INewPreOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly INewOrderValidator _newOrderValidator;

    public NewPreOrderHandler(IOrderRepository orderRepository, IRegionRepository regionRepository,
        ICustomerRepository customerRepository, INewOrderValidator newOrderValidator)
    {
        _orderRepository = orderRepository;
        _regionRepository = regionRepository;
        _customerRepository = customerRepository;
        _newOrderValidator = newOrderValidator;
    }

    public async Task<HandlerResult> Handle(NewPreOrderEvent preOrderRequest, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await GetCustomer(preOrderRequest, cancellationToken);
            var region = await _regionRepository.FindRegionByNameAsync(
                preOrderRequest.Customer.Address.Region, cancellationToken);
            var order = preOrderRequest.ToDomain(region, customer);

            var shouldBeSentToOutbox = _newOrderValidator.IsValidByDistance(order.CustomerAddress, region.Storage);
            await _orderRepository.CreateOrderAsync(order, shouldBeSentToOutbox, cancellationToken);

            return HandlerResult.Ok;
        }
        catch (Exception exception)
        {
            throw new NewPreOrderException(exception.Message);
        }
    }

    private async Task<CustomerEntity> GetCustomer(NewPreOrderEvent request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(request.Customer.Id, cancellationToken);
        
        return customer;
    }
}