using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eShop.Ordering.Domain.Seedwork;

namespace eShop.Ordering.UnitTests.Application.Commands;

[TestClass]
public class CompleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ILogger<CompleteOrderCommandHandler>> _loggerMock;

    public CompleteOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<CompleteOrderCommandHandler>>();
    }

    [TestMethod]
    public async Task Handle_should_return_false_if_order_is_not_found()
    {
        var fakeCommand = new CompleteOrderCommand(123);
        _orderRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>())).ReturnsAsync((Order)null);
        var handler = new CompleteOrderCommandHandler(_orderRepositoryMock.Object, _loggerMock.Object);
        var result = await handler.Handle(fakeCommand, CancellationToken.None);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Handle_should_return_false_when_saving_entities_fails()
    {
        var fakeOrder = CreateFakeOrder();
        var fakeCommand = new CompleteOrderCommand(fakeOrder.Id);
        _orderRepositoryMock.Setup(x => x.GetAsync(fakeCommand.OrderNumber)).ReturnsAsync(fakeOrder);
        _orderRepositoryMock.Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new CompleteOrderCommandHandler(_orderRepositoryMock.Object, _loggerMock.Object);
        var result = await handler.Handle(fakeCommand, CancellationToken.None);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Handle_should_succeed_when_order_exists_and_save_succeeds()
    {
        var fakeOrder = CreateFakeOrder();
        var fakeCommand = new CompleteOrderCommand(fakeOrder.Id);
        _orderRepositoryMock.Setup(x => x.GetAsync(fakeCommand.OrderNumber)).ReturnsAsync(fakeOrder);
        _orderRepositoryMock.Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CompleteOrderCommandHandler(_orderRepositoryMock.Object, _loggerMock.Object);
        var result = await handler.Handle(fakeCommand, CancellationToken.None);
        Assert.IsTrue(result);
        Assert.AreEqual(OrderStatus.Completed, fakeOrder.OrderStatus);
        _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Once);
    }
    
    private Order CreateFakeOrder()
    {
        var fakeAddress = new Address("street", "city", "state", "country", "zipcode");
        var fakeOrder = new Order("fake-user-id", "fake-user-name", fakeAddress, 1, "1234", "Fake Holder", "123", DateTime.UtcNow.AddYears(1));
        typeof(Entity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance).SetValue(fakeOrder, 123);
        return fakeOrder;
    }
}