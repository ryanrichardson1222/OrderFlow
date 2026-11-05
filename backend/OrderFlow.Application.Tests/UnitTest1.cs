using OrderFlow.Application;

namespace OrderFlow.Application.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsFourSeededOrders()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var orders = await service.GetAllAsync();

        // Assert
        Assert.Equal(4, orders.Count);
        Assert.Contains(orders, order =>
            order.CustomerName == "Ryan Richardson");
        Assert.Contains(orders, order =>
            order.CustomerName == "Alex Morgan");
        Assert.Contains(orders, order =>
            order.CustomerName == "Jordan Blake");
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
        Assert.Equal(1, order.UserId);
        Assert.Equal("Ryan Richardson", order.CustomerName);
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
        var request = new CreateOrderRequest(1, 199.99m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Null(result.Error);
        Assert.Equal(5, result.Order.Id);
        Assert.Equal(1, result.Order.UserId);
        Assert.Equal("Ryan Richardson", result.Order.CustomerName);
        Assert.Equal("Pending", result.Order.Status);
        Assert.Equal(199.99m, result.Order.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_WithInvalidUserId_RejectsRequest(int userId)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(userId, 100m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("A valid user ID is required.", result.Error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_WithNonexistentUser_RejectsRequest()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(999, 100m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("The specified user does not exist.", result.Error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_WithNegativeTotal_RejectsRequest()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(1, -1m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("Order total cannot be negative.", result.Error);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task Create_WithValidRequest_PassesValuesToRepository()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest(2, 199.99m);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(2, repository.LastUserId);
        Assert.Equal(199.99m, repository.LastTotal);
    }
}
