
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
    public async Task GetOrders_ReturnsOkAndThreeSeededOrders()
    {
        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<OrderResponse>>();

        Assert.NotNull(orders);
        Assert.Equal(3, orders.Count);

        Assert.Contains(orders, order =>
            order.Id == 1 &&
            order.CustomerName == "Acme Corporation");

        Assert.Contains(orders, order =>
            order.Id == 2 &&
            order.CustomerName == "Northstar Logistics");

        Assert.Contains(orders, order =>
            order.Id == 3 &&
            order.CustomerName == "Summit Healthcare");
    }

    [Theory]
    [InlineData("", "Customer name is required.")]
    [InlineData("   ", "Customer name is required.")]
    public async Task CreateOrder_WithBlankCustomerName_ReturnsBadRequest(
        string customerName,
        string expectedError)
    {
        var request = new CreateOrderRequest(customerName, 100m);

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal(expectedError, body.Error);
    }

    [Fact]
    public async Task CreateOrder_WithNegativeTotal_ReturnsBadRequest()
    {
        var request = new CreateOrderRequest("Contoso", -1m);

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
        var request = new CreateOrderRequest("  Contoso  ", 199.99m);

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith("/api/orders/4", response.Headers.Location!.ToString());

        var order = await response.Content
            .ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.Equal(4, order.Id);
        Assert.Equal("Contoso", order.CustomerName);
        Assert.Equal("Pending", order.Status);
        Assert.Equal(199.99m, order.Total);
    }

    private record CreateOrderRequest(string CustomerName, decimal Total);

    private record OrderResponse(
        int Id,
        string CustomerName,
        string Status,
        decimal Total);

    private record ErrorResponse(string Error);
}
