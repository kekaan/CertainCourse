using Google.Protobuf.Collections;
using Grpc.Core;
using CertainCourse.Customers.Grpc;
using CertainCourse.GatewayService.Infrastructure.Extensions;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Models.Common;
using CertainCourse.GatewayService.Models.Customers.Dtos;

namespace CertainCourse.GatewayService.Models.Customers;

internal sealed class GrpcCustomersService : ICustomersService
{
    private readonly CertainCourse.Customers.Grpc.Customers.CustomersClient _customersGrpcClient;
    private readonly ILogger<GrpcCustomersService> _logger;

    public GrpcCustomersService(CertainCourse.Customers.Grpc.Customers.CustomersClient customersGrpcClient,
        ILogger<GrpcCustomersService> logger)
    {
        _customersGrpcClient = customersGrpcClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CallResult<CustomersDto>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var request = new GetCustomersRequest();

        try
        {
            GetCustomersResponse? response =
                await _customersGrpcClient.GetCustomersAsync(request, cancellationToken: cancellationToken);

            return new CallResult<CustomersDto>(CallResultStatusCode.OK,
                new CustomersDto(GetCustomers(response.Customers)));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(GetCustomersAsync)} call failed: {exception.Message}");
            
            return new CallResult<CustomersDto>(exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    private IReadOnlyCollection<CustomerDto> GetCustomers(RepeatedField<Customer> customersFromResponse)
    {
        return customersFromResponse
            .Select(o => new CustomerDto(
                o.Id,
                o.FirstName,
                o.LastName,
                o.MobileNumber,
                o.Email,
                o.DefaultAddress.ToAddress(),
                ConvertToAddressDtos(o.Addressed).ToArray()))
            .ToArray();
    }

    private IEnumerable<AddressDto> ConvertToAddressDtos(IEnumerable<Address> addresses)
    {
        foreach (Address address in addresses)
        {
            yield return address.ToAddress();
        }
    }
}