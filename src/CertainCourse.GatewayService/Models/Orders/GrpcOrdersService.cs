using Google.Protobuf.Collections;
using Grpc.Core;
using CertainCourse.GatewayService.Infrastructure.Extensions;
using CertainCourse.GatewayService.Infrastructure.ServiceLib;
using CertainCourse.GatewayService.Infrastructure.ServiceLib.Converters;
using CertainCourse.GatewayService.Models.Orders.Dtos;
using CertainCourse.Orders.Grpc;

namespace CertainCourse.GatewayService.Models.Orders;

internal sealed class GrpcOrdersService : IOrdersService
{
    private readonly CertainCourse.Orders.Grpc.Orders.OrdersClient _ordersGrpcClient;
    private readonly ILogger<GrpcOrdersService> _logger;

    public GrpcOrdersService(CertainCourse.Orders.Grpc.Orders.OrdersClient ordersGrpcClient, ILogger<GrpcOrdersService> logger)
    {
        _ordersGrpcClient = ordersGrpcClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CallResult<CancelOrderByIdResponseDto>> CancelOrderByIdAsync(long orderId,
        CancellationToken cancellationToken)
    {
        var request = new CancelOrderByIdRequest
        {
            Id = orderId
        };

        try
        {
            CancelOrderByIdResponse? response =
                await _ordersGrpcClient.CancelOrderByIdAsync(request, cancellationToken: cancellationToken);
            
            return new CallResult<CancelOrderByIdResponseDto>(
                response.IsSuccess ? CallResultStatusCode.OK : CallResultStatusCode.FailedPrecondition,
                new CancelOrderByIdResponseDto(response.IsSuccess, response.ErrorMessage));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(CancelOrderByIdAsync)} call failed: {exception.Message}");
            
            return new CallResult<CancelOrderByIdResponseDto>(exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<CallResult<OrderStateDto>> GetOrderStateByIdAsync(long orderId,
        CancellationToken cancellationToken)
    {
        var request = new GetOrderStateByIdRequest
        {
            Id = orderId
        };

        try
        {
            var response =
                await _ordersGrpcClient.GetOrderStateByIdAsync(request, cancellationToken: cancellationToken);

            return new CallResult<OrderStateDto>(CallResultStatusCode.OK, new OrderStateDto(response.State));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(GetOrderStateByIdAsync)} call failed: {exception.Message}");
            
            return new CallResult<OrderStateDto>(exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<CallResult<RegionsDto>> GetRegionsAsync(CancellationToken cancellationToken)
    {
        var request = new GetRegionsRequest();

        try
        {
            var response = await _ordersGrpcClient.GetRegionsAsync(request, cancellationToken: cancellationToken);

            IReadOnlyCollection<RegionDto> regions = response.Regions
                .Select(e => new RegionDto(e.Id, e.Name))
                .ToArray();

            return new CallResult<RegionsDto>(CallResultStatusCode.OK, new RegionsDto(regions));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(GetRegionsAsync)} call failed: {exception.Message}");

            return new CallResult<RegionsDto>(exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<CallResult<GetOrdersResponseDto>> GetOrdersAsync(GetOrdersRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var request = new GetOrdersRequest
        {
            RegionId = requestDto.RegionId,
            PageSize = requestDto.PageSize,
            PageToken = requestDto.PageToken ?? string.Empty,
            OrderBy = requestDto.OrderBy
        };

        try
        {
            var response = await _ordersGrpcClient.GetOrdersAsync(request, cancellationToken: cancellationToken);

            return new CallResult<GetOrdersResponseDto>(
                CallResultStatusCode.OK,
                new GetOrdersResponseDto(GetOrders(response.Orders), response.NextPageToken));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(GetOrdersAsync)} call failed: {exception.Message}");

            return new CallResult<GetOrdersResponseDto>(exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<CallResult<IReadOnlyCollection<OrdersAggregatedByRegionDto>>> GetOrdersAggregatedByRegionAsync(
        GetOrdersAggregatedByRegionRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var request = new GetOrdersAggregatedByRegionRequest
        {
            StartTime = UnixTimestampConverter.DateTimeToUnixTimeStamp(requestDto.StartTime),
            RegionsIds = { requestDto.RegionsIds }
        };

        try
        {
            var response =
                await _ordersGrpcClient.GetOrdersAggregatedByRegionAsync(request, cancellationToken: cancellationToken);

            return new CallResult<IReadOnlyCollection<OrdersAggregatedByRegionDto>>(
                CallResultStatusCode.OK,
                GetOrdersAggregatedByRegion(response));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(GetOrdersAggregatedByRegionAsync)} call failed: {exception.Message}");

            return new CallResult<IReadOnlyCollection<OrdersAggregatedByRegionDto>>(
                exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<CallResult<GetOrdersByCustomerIdResponseDto>> GetOrdersByCustomerIdAsync(
        GetOrdersByCustomerIdRequestDto requestDto, CancellationToken cancellationToken)
    {
        var request = new GetOrdersByCustomerIdRequest
        {
            CustomerId = requestDto.CustomerId,
            StartTime = UnixTimestampConverter.DateTimeToUnixTimeStamp(requestDto.StartTime),
            PageSize = requestDto.PageSize,
            PageToken = requestDto.PageToken ?? string.Empty,
        };

        try
        {
            var response =
                await _ordersGrpcClient.GetOrdersByCustomerIdAsync(request, cancellationToken: cancellationToken);

            return new CallResult<GetOrdersByCustomerIdResponseDto>(
                CallResultStatusCode.OK,
                new GetOrdersByCustomerIdResponseDto(GetOrders(response.Orders), response.NextPageToken));
        }
        catch (RpcException exception)
        {
            _logger.LogInformation($"{nameof(GetOrdersByCustomerIdAsync)} call failed: {exception.Message}");

            return new CallResult<GetOrdersByCustomerIdResponseDto>(
                exception.StatusCode.ToCallResultStatusCode(),
                errorMessage: exception.Message);
        }
    }

    private IReadOnlyCollection<OrdersAggregatedByRegionDto> GetOrdersAggregatedByRegion(
        GetOrdersAggregatedByRegionResponse response)
    {
        return response.OrdersAggregatedByRegion
            .Select(o => new OrdersAggregatedByRegionDto(
                o.Region,
                o.OrdersCount,
                o.OrdersTotalAmount,
                o.OrdersTotalWeight,
                o.CustomersCount))
            .ToArray();
    }

    private IReadOnlyCollection<OrderDto> GetOrders(RepeatedField<Order> ordersFromResponse)
    {
        return ordersFromResponse
            .Select(o => new OrderDto(
                o.Id,
                o.GoodsCount,
                o.TotalAmount,
                o.TotalWeight,
                o.State.ToOrderState(),
                UnixTimestampConverter.UnixTimeStampToDateTime(o.CreateDate),
                o.Region,
                o.CustomerFirstName,
                o.CustomerLastName,
                o.Address.ToAddress(),
                o.CustomerMobileNumber))
            .ToArray();
    }
}