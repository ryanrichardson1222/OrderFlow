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
    public async Task GetAll_AfterDelete_ExcludesDeletedOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        await service.DeleteAsync(1);

        // Act
        var orders = await service.GetAllAsync();

        // Assert
        Assert.Equal(3, orders.Count);
        Assert.DoesNotContain(orders, order => order.Id == 1);
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
    public async Task GetById_AfterDelete_ReturnsNull()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        await service.DeleteAsync(1);

        // Act
        var order = await service.GetByIdAsync(1);

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

    [Fact]
    public async Task UpdateStatus_WithValidRequest_ReturnsUpdatedOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new UpdateOrderStatusRequest("Shipped");

        // Act
        var result = await service.UpdateStatusAsync(1, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Null(result.Error);
        Assert.Null(result.ErrorCode);
        Assert.Equal(1, result.Order.Id);
        Assert.Equal("Shipped", result.Order.Status);
        Assert.Equal(1, repository.UpdateStatusCallCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateStatus_WithInvalidId_RejectsRequest(int id)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new UpdateOrderStatusRequest("Shipped");

        // Act
        var result = await service.UpdateStatusAsync(id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("A valid order ID is required.", result.Error);
        Assert.Equal(
            UpdateOrderStatusErrorCode.InvalidOrderId,
            result.ErrorCode);
        Assert.Equal(0, repository.UpdateStatusCallCount);
    }

    [Theory]
    [InlineData("Delivered")]
    [InlineData("OnHold")]
    [InlineData("")]
    public async Task UpdateStatus_WithInvalidStatus_RejectsRequest(string status)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new UpdateOrderStatusRequest(status);

        // Act
        var result = await service.UpdateStatusAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("The specified order status is invalid.", result.Error);
        Assert.Equal(
            UpdateOrderStatusErrorCode.InvalidStatus,
            result.ErrorCode);
        Assert.Equal(0, repository.UpdateStatusCallCount);
    }

    [Fact]
    public async Task UpdateStatus_WithUnknownId_RejectsRequest()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new UpdateOrderStatusRequest("Shipped");

        // Act
        var result = await service.UpdateStatusAsync(999, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal("The specified order does not exist.", result.Error);
        Assert.Equal(
            UpdateOrderStatusErrorCode.OrderNotFound,
            result.ErrorCode);
        Assert.Equal(1, repository.UpdateStatusCallCount);
    }

    [Theory]
    [InlineData("shipped")]
    [InlineData(" SHIPPED ")]
    [InlineData("sHiPpEd")]
    public async Task UpdateStatus_WithValidStatus_NormalizesStatus(string status)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);
        var request = new UpdateOrderStatusRequest(status);

        // Act
        var result = await service.UpdateStatusAsync(1, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Equal("Shipped", result.Order.Status);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsDeletedOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Equal(1, result.Order.Id);
        Assert.Null(result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Delete_WithInvalidId_ReturnsInvalidOrderIdError(int id)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var result = await service.DeleteAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal(
            OrderMutationErrorCode.InvalidOrderId,
            result.ErrorCode);
    }

    [Fact]
    public async Task Restore_WithDeletedId_ReturnsRestoredOrder()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        await service.DeleteAsync(1);

        // Act
        var result = await service.RestoreAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Order);
        Assert.Equal(1, result.Order.Id);
        Assert.Null(result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Restore_WithInvalidId_ReturnsInvalidOrderIdError(int id)
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var result = await service.RestoreAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal(
            OrderMutationErrorCode.InvalidOrderId,
            result.ErrorCode);
    }

    [Fact]
    public async Task GetAllIncludingDeleted_ReturnsDeletedOrdersWithDeletionStatus()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        await service.DeleteAsync(1);

        // Act
        var orders = await service.GetAllIncludingDeletedAsync();

        // Assert
        Assert.Equal(4, orders.Count);

        var deletedOrder = Assert.Single(
            orders,
            order => order.Order.Id == 1);

        Assert.True(deletedOrder.IsDeleted);
        Assert.NotNull(deletedOrder.DeletedAt);
        Assert.Equal(1, deletedOrder.Order.Id);
    }

    [Fact]
    public async Task Delete_WithAlreadyDeletedId_ReturnsAlreadyDeletedError()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        await service.DeleteAsync(1);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal(
            OrderMutationErrorCode.AlreadyDeleted,
            result.ErrorCode);
    }

    [Fact]
    public async Task Restore_WithActiveId_ReturnsNotDeletedError()
    {
        // Arrange
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        // Act
        var result = await service.RestoreAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Order);
        Assert.Equal(
            OrderMutationErrorCode.NotDeleted,
            result.ErrorCode);
    }
}
