using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Application;
using OrderFlow.Infrastructure;

namespace OrderFlow.Api.Tests;

public class OrderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiTests(WebApplicationFactory<Program> factory)
{
    var configuredFactory = factory.WithWebHostBuilder(builder =>
    {
        builder.UseSetting("https_port", "5001");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOrderRepository>();

            services.AddSingleton<
                IOrderRepository,
                InMemoryOrderRepository>();
        });
    });

    _client = configuredFactory.CreateClient();
}

    [Fact]
    public async Task GetOrders_ReturnsOkAndFourSeededOrders()
    {
        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<OrderResponse>>();

        Assert.NotNull(orders);
        Assert.Equal(4, orders.Count);

        Assert.Contains(orders, order =>
            order.Id == 1 &&
            order.UserId == 1 &&
            order.CustomerName == "Ryan Richardson");

        Assert.Contains(orders, order =>
            order.Id == 3 &&
            order.UserId == 2 &&
            order.CustomerName == "Alex Morgan");

        Assert.Contains(orders, order =>
            order.Id == 4 &&
            order.UserId == 3 &&
            order.CustomerName == "Jordan Blake");
    }

    [Fact]
    public async Task DeleteAndRestoreOrder_UpdatesOrderLifecycle()
    {
        // Arrange
        const int orderId = 1;

        // Act: Delete the order
        var deleteResponse = await _client.DeleteAsync(
            $"/api/orders/{orderId}");

        // Assert: Delete succeeded
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act: Verify normal retrieval excludes deleted orders
        var getResponse = await _client.GetAsync("/api/orders");

        var activeOrders = await getResponse.Content
            .ReadFromJsonAsync<List<OrderResponse>>();

        Assert.NotNull(activeOrders);
        Assert.DoesNotContain(activeOrders, order => order.Id == orderId);

        // Act: Restore the order
        var restoreResponse = await _client.PostAsync(
            $"/api/orders/{orderId}/restore",
            null);

        // Assert: Restore succeeded
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // Act: Verify the order appears again
        getResponse = await _client.GetAsync("/api/orders");

        activeOrders = await getResponse.Content
            .ReadFromJsonAsync<List<OrderResponse>>();

        Assert.NotNull(activeOrders);
        Assert.Contains(activeOrders, order => order.Id == orderId);
    }


    [Fact]
    public async Task GetAllOrdersIncludingDeleted_ReturnsDeletionMetadata()
    {
        // Arrange: Delete order 1
        var deleteResponse = await _client.DeleteAsync("/api/orders/1");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Act: Retrieve all orders, including deleted ones
        var response = await _client.GetAsync(
        "/api/orders/admin/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<OrderWithDeletionStatus>>();

        Assert.NotNull(orders);
        Assert.Equal(4, orders.Count);

        var deletedOrder = Assert.Single(
            orders,
            order => order.Order.Id == 1);

        Assert.True(deletedOrder.IsDeleted);
        Assert.NotNull(deletedOrder.DeletedAt);
        Assert.Equal("Ryan Richardson", deletedOrder.Order.CustomerName);
    }


    [Fact]
    public async Task CreateOrder_WithInvalidUserId_ReturnsBadRequest()
    {
        var request = new CreateOrderRequest(0, 100m);

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("A valid user ID is required.", body.Error);
    }

    [Fact]
    public async Task CreateOrder_WithNonexistentUser_ReturnsBadRequest()
    {
        var request = new CreateOrderRequest(999, 100m);

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified user does not exist.", body.Error);
    }

    [Fact]
    public async Task CreateOrder_WithNegativeTotal_ReturnsBadRequest()
    {
        var request = new CreateOrderRequest(1, -1m);

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("Order total cannot be negative.", body.Error);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ReturnsCreatedOrder()
    {
        var request = new CreateOrderRequest(1, 199.99m);

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith(
            "/api/orders/5",
            response.Headers.Location!.ToString());

        var order = await response.Content
            .ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal(5, order.Id);
        Assert.Equal(1, order.UserId);
        Assert.Equal("Ryan Richardson", order.CustomerName);
        Assert.Equal("Pending", order.Status);
        Assert.Equal(199.99m, order.Total);
    }


    [Fact]
    public async Task UpdateOrderStatus_WithValidRequest_ReturnsUpdatedOrder()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest("Shipped");

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/orders/1/status",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content
            .ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal(1, order.Id);
        Assert.Equal("Shipped", order.Status);
    }


    [Fact]
    public async Task UpdateOrderStatus_WithInvalidId_ReturnsBadRequest()
    {
            // Arrange
            var request = new UpdateOrderStatusRequest("Shipped");

            // Act
            var response = await _client.PutAsJsonAsync(
                "/api/orders/0/status",
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content
                .ReadFromJsonAsync<ErrorResponse>();

            Assert.NotNull(body);
            Assert.Equal("A valid order ID is required.", body.Error);
    }


    [Fact]
    public async Task UpdateOrderStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest("Delivered");

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/orders/1/status",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified order status is invalid.", body.Error);
    }


    [Fact]
    public async Task UpdateOrderStatus_WithUnknownId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest("Shipped");

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/orders/999/status",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified order does not exist.", body.Error);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithDeletedOrder_ReturnsConflict()
    {
        // Arrange
        var deleteResponse = await _client.DeleteAsync("/api/orders/1");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var request = new UpdateOrderStatusRequest("Shipped");

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/orders/1/status",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal(
            "Cannot update an order that has been deleted.",
            body.Error);
    }

    [Fact]
    public async Task DeleteOrder_WithInvalidId_ReturnsBadRequest()
    {
        var response = await _client.DeleteAsync("/api/orders/0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("A valid order ID is required.", body.Error);
    }

    [Fact]
    public async Task DeleteOrder_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/orders/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified order does not exist.", body.Error);
    }

    [Fact]
    public async Task DeleteOrder_WhenAlreadyDeleted_ReturnsConflict()
    {
        var firstResponse = await _client.DeleteAsync("/api/orders/1");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await _client.DeleteAsync("/api/orders/1");

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var body = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified order has already been deleted.", body.Error);
    }

    [Fact]
    public async Task RestoreOrder_WithInvalidId_ReturnsBadRequest()
    {
        var response = await _client.PostAsync(
            "/api/orders/0/restore", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("A valid order ID is required.", body.Error);
    }

    [Fact]
    public async Task RestoreOrder_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.PostAsync(
            "/api/orders/999/restore", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified order does not exist.", body.Error);
    }

    [Fact]
    public async Task RestoreOrder_WhenOrderIsNotDeleted_ReturnsConflict()
    {
        var response = await _client.PostAsync(
            "/api/orders/1/restore", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("The specified order is not deleted.", body.Error);
    }

    private record CreateOrderRequest(int UserId, decimal Total);

    private record OrderResponse(
        int Id,
        int UserId,
        string CustomerName,
        string Status,
        decimal Total);

    private record ErrorResponse(string Error);
}
