using Moq;
using CertainCourse.OrderService.Application.DataAccess.Interfaces;
using CertainCourse.OrderService.Application.DataAccess.Models;
using CertainCourse.OrderService.Application.NewPreOrder;
using CertainCourse.OrderService.Application.Validators.Orders;
using CertainCourse.OrderService.Domain;
using CertainCourse.OrderService.Domain.ValueObjects.Region;
using TestsCommon;
using Xunit;
using DomainAddress = CertainCourse.OrderService.Domain.ValueObjects.Customer.Address;

namespace CertainCourse.OrderService.Application.UnitTests.HandlersTests;

public class NewPreOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IRegionRepository> _mockRegionRepository;
    private readonly Mock<INewOrdersOutboxRepository> _mockOutboxRepository;
    private readonly Mock<INewOrderValidator> _mockNewOrderValidator;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly NewPreOrderHandler _handler;

    public NewPreOrderHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockRegionRepository = new Mock<IRegionRepository>();
        _mockOutboxRepository = new Mock<INewOrdersOutboxRepository>();
        _mockNewOrderValidator = new Mock<INewOrderValidator>();

        _handler = new NewPreOrderHandler(
            _mockOrderRepository.Object,
            _mockRegionRepository.Object,
            _mockCustomerRepository.Object,
            _mockNewOrderValidator.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenPreOrderIsValid()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var preOrder = TestEntities.PreOrder;
        var region = new RegionEntity(
            new RegionId(1),
            Name: preOrder.Customer.Address.Region,
            Storage: new Storage(preOrder.Customer.Address.Latitude,
                preOrder.Customer.Address.Longitude));

        _mockCustomerRepository
            .Setup(x => x.GetCustomerByIdAsync(
                It.IsAny<int>(),
                cancellationToken))
            .ReturnsAsync(TestEntities.Customer);

        _mockRegionRepository
            .Setup(x => x.FindRegionByNameAsync(
                It.IsAny<string>(),
                cancellationToken))
            .ReturnsAsync(region);

        _mockOrderRepository
            .Setup(x => x.CreateOrderAsync(
                It.IsAny<OrderEntity>(),
                It.IsAny<bool>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockNewOrderValidator
            .Setup(x => x.IsValidByDistance(
                It.IsAny<DomainAddress>(),
                It.IsAny<Storage>()))
            .Returns(true);

        _mockOutboxRepository
            .Setup(x => x.InsertAsync(
                It.IsAny<NewOrdersOutboxMessage>(),
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(preOrder, cancellationToken);

        // Assert
        Assert.True(result.Success);

        _mockCustomerRepository.Verify(
            x => x.GetCustomerByIdAsync(It.IsAny<int>(), cancellationToken), Times.Once);
        _mockRegionRepository.Verify(x => x.FindRegionByNameAsync(It.IsAny<string>(), cancellationToken), Times.Once);
        _mockOrderRepository.Verify(x => x.CreateOrderAsync(It.IsAny<OrderEntity>(), It.IsAny<bool>(), cancellationToken), Times.Once);
        _mockNewOrderValidator.Verify(x => x.IsValidByDistance(It.IsAny<DomainAddress>(), It.IsAny<Storage>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotAddToOutbox_WhenOrderIsInvalidByDistance()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var preOrder = TestEntities.PreOrder;
        var region = new RegionEntity(
            new RegionId(1),
            Name: preOrder.Customer.Address.Region,
            Storage: new Storage(preOrder.Customer.Address.Latitude,
                preOrder.Customer.Address.Longitude));

        _mockCustomerRepository
            .Setup(x => x.GetCustomerByIdAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(TestEntities.Customer);

        _mockRegionRepository
            .Setup(x => x.FindRegionByNameAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(region);

        _mockOrderRepository
            .Setup(x => x.CreateOrderAsync(It.IsAny<OrderEntity>(), It.IsAny<bool>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockNewOrderValidator
            .Setup(x => x.IsValidByDistance(It.IsAny<DomainAddress>(), It.IsAny<Storage>()))
            .Returns(false);

        // Act
        var result = await _handler.Handle(preOrder, cancellationToken);

        // Assert
        Assert.True(result.Success);

        _mockCustomerRepository.Verify(
            x => x.GetCustomerByIdAsync(It.IsAny<int>(), cancellationToken), Times.Once);
        _mockRegionRepository.Verify(x => x.FindRegionByNameAsync(It.IsAny<string>(), cancellationToken), Times.Once);
        _mockOrderRepository.Verify(x => x.CreateOrderAsync(It.IsAny<OrderEntity>(), It.IsAny<bool>(), cancellationToken), Times.Once);
        _mockNewOrderValidator.Verify(x => x.IsValidByDistance(It.IsAny<DomainAddress>(), It.IsAny<Storage>()),
            Times.Once);
        _mockOutboxRepository.Verify(x => x.InsertAsync(It.IsAny<NewOrdersOutboxMessage>(), cancellationToken), Times.Never);
    }
}