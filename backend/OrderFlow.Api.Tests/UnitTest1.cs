using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace OrderFlow.Api.Tests;

public class OrderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiTests(WebApplicationFactory<Program> factory)
    {
        var configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("https_port", "5001");
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

    private record CreateOrderRequest(int UserId, decimal Total);

    private record OrderResponse(
        int Id,
        int UserId,
        string CustomerName,
        string Status,
        decimal Total);

    private record ErrorResponse(string Error);
}
