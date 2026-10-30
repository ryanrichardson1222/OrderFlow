using OrderFlow.Application;
using OrderFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/orders", async (
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var orders = await service.GetAllAsync(cancellationToken);

    return Results.Ok(orders);
})
.WithName("GetOrders");

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.CreateAsync(
        request,
        cancellationToken);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { error = result.Error });
    }

    return Results.Created(
        $"/api/orders/{result.Order!.Id}",
        result.Order);
})
.WithName("CreateOrder");

app.MapGet("/api/orders/{id:int}", async (
    int id,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var order = await service.GetByIdAsync(
        id,
        cancellationToken);

    if (order is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
})
.WithName("GetOrderById");

app.Run();

public partial class Program { }
