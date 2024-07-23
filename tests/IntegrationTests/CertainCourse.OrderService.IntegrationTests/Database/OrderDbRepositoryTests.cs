using CertainCourse.OrderService.Common.RequestsSpecifications;
using CertainCourse.OrderService.Common.RequestsSpecifications.Orders;
using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Order;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Dals;
using CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Repositories.Base;

namespace CertainCourse.OrderService.IntegrationTests.Database;

[Collection("Sequential")]
public class OrderDbRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly OrderDbRepository _orderDbRepository;
    private readonly DatabaseFixture _fixture;

    public OrderDbRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _orderDbRepository = new OrderDbRepository(_fixture.ConnectionFactory);
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertOrder()
    {
        // Arrange
        var order = new OrderDal
        {
            Id = 1,
            GoodsCount = 10,
            TotalAmount = 100,
            TotalWeight = 50,
            Type = OrderType.Api,
            CreateDate = DateTime.UtcNow,
            RegionId = 1,
            State = OrderState.Created,
            CustomerId = 1
        };

        // Act
        await _orderDbRepository.InsertAsync(order,false, CancellationToken.None);

        var insertedOrder = await _orderDbRepository.GetByIdAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(insertedOrder);
        Assert.Equal(order.Id, insertedOrder.Id);
        Assert.Equal(order.GoodsCount, insertedOrder.GoodsCount);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var order = new OrderDal
        {
            Id = 1,
            GoodsCount = 10,
            TotalAmount = 100,
            TotalWeight = 50,
            Type = OrderType.Api,
            CreateDate = DateTime.UtcNow,
            RegionId = 1,
            State = OrderState.Created,
            CustomerId = 1
        };

        // Act
        await _orderDbRepository.InsertAsync(order,false, CancellationToken.None);

        var fetchedOrder = await _orderDbRepository.GetByIdAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(fetchedOrder);
        Assert.Equal(order.Id, fetchedOrder.Id);
        Assert.Equal(order.GoodsCount, fetchedOrder.GoodsCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrder()
    {
        // Arrange
        var order = new OrderDal
        {
            Id = 1,
            GoodsCount = 10,
            TotalAmount = 100,
            TotalWeight = 50,
            Type = OrderType.Api,
            CreateDate = DateTime.UtcNow,
            RegionId = 1,
            State = OrderState.Created,
            CustomerId = 1
        };

        var changedOrder = order with
        {
            GoodsCount = 15,
            TotalAmount = 150
        };

        // Act
        await _orderDbRepository.InsertAsync(order,false, CancellationToken.None);
        await _orderDbRepository.UpdateAsync(changedOrder, CancellationToken.None);

        var updatedOrder = await _orderDbRepository.GetByIdAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(updatedOrder);
        Assert.Equal(changedOrder.GoodsCount, updatedOrder.GoodsCount);
        Assert.Equal(changedOrder.TotalAmount, updatedOrder.TotalAmount);
    }

    private async Task InsertTestOrdersAsync()
    {
        var orders = new List<OrderDal>
        {
            new()
            {
                Id = 1, GoodsCount = 10, TotalAmount = 100, TotalWeight = 50, Type = OrderType.Web,
                CreateDate = DateTime.UtcNow, RegionId = 1, State = OrderState.Created, CustomerId = 1,
                CustomerFirstName = "Vasya", CustomerLastName = "Pupkin", CustomerMobileNumber = "88005553535",
                CustomerAddress = new Address
                {
                    Region = "Moscow",
                    City = "Moscow",
                    Street = "Tverskaya",
                    Building = "12",
                    Apartment = "34",
                    Latitude = 55.7558,
                    Longitude = 37.6173
                }
            },
            new()
            {
                Id = 2, GoodsCount = 5, TotalAmount = 50, TotalWeight = 25, Type = OrderType.Mobile,
                CreateDate = DateTime.UtcNow.AddDays(-1), RegionId = 2, State = OrderState.SentToCustomer,
                CustomerId = 2, CustomerFirstName = "Lesha", CustomerLastName = "Pupkin", CustomerAddress = new Address
                {
                    Region = "Moscow",
                    City = "Moscow",
                    Street = "Tverskaya",
                    Building = "12",
                    Apartment = "34",
                    Latitude = 55.7558,
                    Longitude = 37.6173
                }
            },
            new()
            {
                Id = 3, GoodsCount = 8, TotalAmount = 80, TotalWeight = 40, Type = OrderType.Api,
                CreateDate = DateTime.UtcNow.AddDays(-2), RegionId = 1, State = OrderState.Delivered, CustomerId = 3,
                CustomerFirstName = "Lena", CustomerLastName = "Pupkina", CustomerAddress = new Address
                {
                    Region = "Moscow",
                    City = "Moscow",
                    Street = "Tverskaya",
                    Building = "12",
                    Apartment = "34",
                    Latitude = 55.7558,
                    Longitude = 37.6173
                }
            }
        };

        foreach (var order in orders)
        {
            await _orderDbRepository.InsertAsync(order, false, CancellationToken.None);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrders_WhenNoParametersProvided()
    {
        // Arrange
        await InsertTestOrdersAsync();

        // Act
        var orders = await _orderDbRepository.GetAllAsync(CancellationToken.None);

        Assert.NotNull(orders);
        Assert.Equal(3, orders.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFilteredOrders()
    {
        // Arrange
        await InsertTestOrdersAsync();
        var filteringSpecifications = new List<OrderFilteringSpecification>
        {
            new() { RegionIdPossibleValues = new[] { 1 }, CustomerIdPossibleValues = new[] { 1 } }
        };

        // Act
        var orders = await _orderDbRepository.GetAllAsync(
            CancellationToken.None,
            null,
            filteringSpecifications);

        // Assert
        Assert.NotNull(orders);
        Assert.Single(orders.Items);
        Assert.Equal(1, orders.Items.First().CustomerId);
        Assert.Equal(1, orders.Items.First().RegionId);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSortedOrders()
    {
        // Arrange
        await InsertTestOrdersAsync();
        var sortingSpecifications = new List<OrderSortingSpecification>
        {
            new(OrderProperty.CreateDate, true)
        };

        // Act
        var orders = await _orderDbRepository.GetAllAsync(
            CancellationToken.None,
            null,
            null,
            sortingSpecifications);

        // Assert
        Assert.NotNull(orders);
        Assert.Equal(3, orders.Items.Count);
        Assert.True(orders.Items.First().CreateDate > orders.Items.Last().CreateDate);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedOrders()
    {
        // Arrange
        await InsertTestOrdersAsync();
        var paginationSpecification = new PaginationSpecification(2, "0");

        // Act
        var orders = await _orderDbRepository.GetAllAsync(
            CancellationToken.None,
            paginationSpecification);

        // Assert
        Assert.NotNull(orders);
        Assert.Equal(2, orders.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFilteredAndSortedOrders()
    {
        // Arrange
        await InsertTestOrdersAsync();
        var filteringSpecifications = new List<OrderFilteringSpecification>
        {
            new() { RegionIdPossibleValues = new[] { 1 } }
        };
        var sortingSpecifications = new List<OrderSortingSpecification>
        {
            new(OrderProperty.CreateDate, true)
        };

        // Act
        var orders = await _orderDbRepository.GetAllAsync(
            CancellationToken.None,
            null,
            filteringSpecifications,
            sortingSpecifications);

        // Assert
        Assert.NotNull(orders);
        Assert.Equal(2, orders.Items.Count);
        Assert.True(orders.Items.First().CreateDate > orders.Items.Last().CreateDate);
    }

    public void Dispose() => _fixture.Dispose();
}