using Grpc.Core;
using CertainCourse.Orders.Grpc;
using static CertainCourse.Orders.Grpc.Orders;
using StatusProto = Grpc.Core.Status;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.LogisticSimulatorClient;
using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Extensions.EntitiesExtensions;

namespace CertainCourse.OrderService.Infrastructure.Grpc.Services;

internal sealed class OrdersService : OrdersBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ILogisticSimulatorClient _logisticsSimulatorClient;
    private readonly ICustomerRepository _customerRepository;

    public OrdersService(IOrderRepository orderRepository, IRegionRepository regionRepository,
        ILogisticSimulatorClient logisticsSimulatorClient, ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _regionRepository = regionRepository;
        _logisticsSimulatorClient = logisticsSimulatorClient;
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Отменить заказ по его Id
    /// </summary>
    /// <param name="request">Объект запрос</param>
    /// <param name="context">Контекст запроса</param>
    /// <returns></returns>
    public override async Task<CancelOrderByIdResponse> CancelOrderById(CancelOrderByIdRequest request,
        ServerCallContext context)
    {
        // в дальнейшем будет необходимо обернуть эти действия в транзакцию БД и применить outbox
        try
        {
            var logsticSimulatorCancelResult =
                await  _logisticsSimulatorClient.CancelOrderAsync(request.Id, context.CancellationToken);

            if (!logsticSimulatorCancelResult.IsSuccess)
            {
                return WrapFailedCancellingOnLogistic(logsticSimulatorCancelResult);
            }

            // отмена заказа после успешной отмене в LogisticSimulator
            await _orderRepository.CancelOrderAsync(request.Id, context.CancellationToken);

            return new CancelOrderByIdResponse
            {
                IsSuccess = true
            };
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new StatusProto(
                StatusCode.NotFound,
                $"There are no orders with this ID: {request.Id}"));
        }
    }

    private CancelOrderByIdResponse WrapFailedCancellingOnLogistic(LogisticSimulatorCancelOrderResult logisticSimulatorCancelOrderResult)
    {
        return new CancelOrderByIdResponse
        {
            IsSuccess = false,
            ErrorMessage =
                $"Cancelling order on logistic simulator error: {logisticSimulatorCancelOrderResult.ErrorMessage}"
        };
    }

    /// <summary>
    /// Получить статус заказа по его Id
    /// </summary>
    /// <param name="request">Объект запрос</param>
    /// <param name="context">Контекст запроса</param>
    /// <returns></returns>
    public override async Task<GetOrderStateByIdResponse> GetOrderStateById(GetOrderStateByIdRequest request,
        ServerCallContext context)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(request.Id, context.CancellationToken);

            return new GetOrderStateByIdResponse
            {
                State = order.State.ToOrderStateProto()
            };
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new StatusProto(
                StatusCode.NotFound,
                $"There are no orders with this ID: {request.Id}"));
        }
    }

    /// <summary>
    /// Получить список всех регионов
    /// </summary>
    /// <param name="request">Объект запрос</param>
    /// <param name="context">Контекст запроса</param>
    /// <returns></returns>
    public override async Task<GetRegionsResponse> GetRegions(GetRegionsRequest request, ServerCallContext context)
    {
        var regions = await _regionRepository.GetRegionsAsync(context.CancellationToken);

        return new GetRegionsResponse
        {
            Regions =
            {
                regions.Select(e => e.ToRegionProto()).ToArray()
            }
        };
    }

    /// <summary>
    /// Получить все ордера исходя из заданных параметров.
    /// </summary>
    /// <param name="request">Объект запрос</param>
    /// <param name="context">Контекст запроса</param>
    /// <returns></returns>
    public override async Task<GetOrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        var regionExists = await _regionRepository.IsRegionExistAsync(request.RegionId, context.CancellationToken);

        if (!regionExists)
        {
            throw new RpcException(new StatusProto(
                StatusCode.NotFound,
                $"There are no orders with this region: {request.RegionId}"));
        }

        OrderIncludeSpecification includeSpecification = new(IncludeRegionInfo: true);

        IReadOnlyCollection<OrderFilteringSpecification> filteringSpecification = new[]
        {
            new OrderFilteringSpecification { RegionIdPossibleValues = new[] { request.RegionId } }
        };

        IReadOnlyCollection<OrderSortingSpecification> sortingSpecifications = new[]
        {
            new OrderSortingSpecification(
                OrderProperty.RegionId,
                SortDescending: request.OrderBy is "desc")
        };

        var pagedOrders = await _orderRepository.GetOrdersPagedListAsync(
            new PaginationSpecification(request.PageSize, request.PageToken),
            context.CancellationToken,
            includeSpecification: includeSpecification,
            filteringSpecifications: filteringSpecification,
            sortingSpecifications: sortingSpecifications);

        return new GetOrdersResponse
        {
            Orders =
            {
                pagedOrders.Items.Select(e => e.ToProto())
            },
            NextPageToken = pagedOrders.NextPageToken
        };
    }

    /// <summary>
    /// Получить агрегат заказов для определенного региона
    /// </summary>
    /// <param name="request">Объект запрос</param>
    /// <param name="context">Контекст запроса</param>
    /// <returns></returns>
    public override async Task<GetOrdersAggregatedByRegionResponse> GetOrdersAggregatedByRegion(
        GetOrdersAggregatedByRegionRequest request, ServerCallContext context)
    {
        var regionsIds = request.RegionsIds;
        var regionsExist = await AreRegionsExist(regionsIds, context.CancellationToken);

        if (!regionsExist)
        {
            throw new RpcException(new StatusProto(
                StatusCode.NotFound,
                $"Some of regions not exists: {regionsIds}"));
        }

        OrderIncludeSpecification includeSpecification = new(IncludeRegionInfo: true);

        IReadOnlyCollection<OrderFilteringSpecification> filteringSpecification = new[]
        {
            new OrderFilteringSpecification
            {
                MinCreateDateTime = UnixTimestampConverter.UnixTimeStampToDateTime(request.StartTime),
                RegionIdPossibleValues = regionsIds.ToArray()
            }
        };

        var orders = await _orderRepository.GetOrdersAsync(
            context.CancellationToken,
            includeSpecification: includeSpecification,
            filteringSpecifications: filteringSpecification);

        var aggregatedOrders = orders.GroupBy(e => e.RegionName);

        return new GetOrdersAggregatedByRegionResponse
        {
            OrdersAggregatedByRegion = { GetOrdersAggregatedByRegionProto(aggregatedOrders) }
        };
    }

    private async Task<bool> AreRegionsExist(IEnumerable<int> regionIds, CancellationToken cancellationToken)
    {
        foreach (var regionId in regionIds)
        {
            if (!await _regionRepository.IsRegionExistAsync(regionId, cancellationToken))
                return false;
        }

        return true;
    }

    private IReadOnlyCollection<OrdersAggregatedByRegion> GetOrdersAggregatedByRegionProto(
        IEnumerable<IGrouping<string, OrderEntity>> groupedOrders)
    {
        return groupedOrders
            .Select(go => go.ToOrdersAggregatedByRegionProto())
            .ToArray();
    }

    /// <summary>
    /// Получить список заказов по Id клиента
    /// </summary>
    /// <param name="request">Объект запрос</param>
    /// <param name="context">Контекст запроса</param>
    /// <returns></returns>
    public override async Task<GetOrdersByCustomerIdResponse> GetOrdersByCustomerId(
        GetOrdersByCustomerIdRequest request,
        ServerCallContext context)
    {
        await CheckIfCustomerExistsOrThrowAsync(request.CustomerId, context.CancellationToken);

        OrderIncludeSpecification includeSpecification = new(IncludeRegionInfo: true);

        IReadOnlyCollection<OrderFilteringSpecification> filteringSpecification = new[]
        {
            new OrderFilteringSpecification
            {
                MinCreateDateTime = UnixTimestampConverter.UnixTimeStampToDateTime(request.StartTime),
                CustomerIdPossibleValues = new[] { request.CustomerId }
            }
        };

        var pagedOrders = await _orderRepository.GetOrdersPagedListAsync(
            new PaginationSpecification(request.PageSize, request.PageToken),
            context.CancellationToken,
            includeSpecification: includeSpecification,
            filteringSpecifications: filteringSpecification);

        return new GetOrdersByCustomerIdResponse
        {
            Orders = { pagedOrders.Items.Select(e => e.ToProto()).ToArray() },
            NextPageToken = pagedOrders.NextPageToken
        };
    }

    private async Task CheckIfCustomerExistsOrThrowAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            await _customerRepository.GetCustomerByIdAsync(customerId, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new RpcException(new StatusProto(StatusCode.Unknown, $"Failed to get customer info: {exception.Message}"));
        }
    }
}