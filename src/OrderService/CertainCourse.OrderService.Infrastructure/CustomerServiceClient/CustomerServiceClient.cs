using CertainCourse.Customers.Grpc;
using CertainCourse.OrderService.Application.CustomerServiceClient;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Infrastructure.DataAccess.EntitiesExtensions;
using GrpcCustomersClient = CertainCourse.Customers.Grpc.Customers.CustomersClient;

namespace CertainCourse.OrderService.Infrastructure.CustomerServiceClient;

public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly GrpcCustomersClient _customersGrpcClient;
    
    public CustomerServiceClient(GrpcCustomersClient customersGrpcClient)
    {
        _customersGrpcClient = customersGrpcClient;
    }
    
    public async Task<CustomerEntity> GetCustomer(int id, CancellationToken cancellationToken)
    {
        var grpcRequest = new GetCustomerByIdRequest
        {
            Id = id
        };

        var customerResponse =
            await _customersGrpcClient.GetCustomerByIdAsync(grpcRequest, cancellationToken: cancellationToken);

        return customerResponse.Customer.ToDomain();
    }
}