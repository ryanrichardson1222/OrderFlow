using OrderFlow.Application;

namespace OrderFlow.Application.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsThreeSeededOrders()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var orders = await service.GetAllAsync();

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
    public async Task GetById_WithExistingId_ReturnsMatchingOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var order = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(order);
        Assert.Equal("Acme Corporation", order.CustomerName);
        Assert.Equal("Processing", order.Status);
        Assert.Equal(249.99m, order.Total);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNull()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var order = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(order);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsNewOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("  Contoso  ", 199.99m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Null(result.Error);
        Assert.Equal(4, result.Order.Id);
        Assert.Equal("Contoso", result.Order.CustomerName);
        Assert.Equal("Pending", result.Order.Status);
        Assert.Equal(199.99m, result.Order.Total);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithBlankCustomerName_RejectsRequest(
        string customerName)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(customerName, 100m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("Customer name is required.", result.Error);

        var orders = await service.GetAllAsync();
        Assert.Equal(3, orders.Count);
    }

    [Fact]
    public async Task Create_WithNegativeTotal_RejectsRequest()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("Contoso", -1m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("Order total cannot be negative.", result.Error);

        var orders = await service.GetAllAsync();
        Assert.Equal(3, orders.Count);
    }

    [Fact]
    public async Task Create_WithBlankCustomerName_DoesNotCallRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(" ", 100m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Customer name is required.", result.Error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_WithNegativeTotal_DoesNotCallRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("Contoso", -1m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order total cannot be negative.", result.Error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_WithValidRequest_PassesTrimmedValuesToRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest("  Contoso  ", 199.99m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Null(result.Error);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal("Contoso", repository.LastCustomerName);
        Assert.Equal(199.99m, repository.LastTotal);
    }
}
