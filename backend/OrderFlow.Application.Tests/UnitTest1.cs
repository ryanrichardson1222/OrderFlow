using OrderFlow.Application;

namespace OrderFlow.Application.Tests;

public class OrderServiceTests
{
    [Fact]
    public void GetAll_ReturnsThreeSeededOrders()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var orders = service.GetAll();

        // Assert
        Assert.Equal(3, orders.Count);
        Assert.Contains(orders, order =>
            order.CustomerName == "Acme Corporation");
        Assert.Contains(orders, order =>
            order.CustomerName == "Northstar Logistics");
        Assert.Contains(orders, order =>
            order.CustomerName == "Summit Healthcare");
    }

    [Fact]
    public void GetById_WithExistingId_ReturnsMatchingOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var order = service.GetById(1);

        // Assert
        Assert.NotNull(order);
        Assert.Equal("Acme Corporation", order.CustomerName);
        Assert.Equal("Processing", order.Status);
        Assert.Equal(249.99m, order.Total);
    }

    [Fact]
    public void GetById_WithUnknownId_ReturnsNull()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var order = service.GetById(999);

        // Assert
        Assert.Null(order);
    }

    [Fact]
    public void Create_WithValidRequest_ReturnsNewOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("  Contoso  ", 199.99m);

        // Act
        var order = service.Create(request, out var error);

        // Assert
        Assert.NotNull(order);
        Assert.Null(error);
        Assert.Equal(4, order.Id);
        Assert.Equal("Contoso", order.CustomerName);
        Assert.Equal("Pending", order.Status);
        Assert.Equal(199.99m, order.Total);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankCustomerName_RejectsRequest(
        string customerName)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(customerName, 100m);

        // Act
        var order = service.Create(request, out var error);

        // Assert
        Assert.Null(order);
        Assert.Equal("Customer name is required.", error);
        Assert.Equal(3, service.GetAll().Count);
    }

    [Fact]
    public void Create_WithNegativeTotal_RejectsRequest()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("Contoso", -1m);

        // Act
        var order = service.Create(request, out var error);

        // Assert
        Assert.Null(order);
        Assert.Equal("Order total cannot be negative.", error);
        Assert.Equal(3, service.GetAll().Count);
    }

    [Fact]
    public void Create_WithBlankCustomerName_DoesNotCallRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(" ", 100m);

        // Act
        var order = service.Create(request, out var error);

        // Assert
        Assert.Null(order);
        Assert.Equal("Customer name is required.", error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public void Create_WithNegativeTotal_DoesNotCallRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("Contoso", -1m);

        // Act
        var order = service.Create(request, out var error);

        // Assert
        Assert.Null(order);
        Assert.Equal("Order total cannot be negative.", error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public void Create_WithValidRequest_PassesTrimmedValuesToRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("  Contoso  ", 199.99m);

        // Act
        var order = service.Create(request, out var error);

        // Assert
        Assert.NotNull(order);
        Assert.Null(error);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal("Contoso", repository.LastCustomerName);
        Assert.Equal(199.99m, repository.LastTotal);
    }
}
