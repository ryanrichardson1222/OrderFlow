
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

app.MapGet("/api/orders", (OrderService service) =>
{
    return Results.Ok(service.GetAll());
})
.WithName("GetOrders");

app.MapPost("/api/orders", (CreateOrderRequest request, OrderService service) =>
{
    var order = service.Create(request, out var error);

    if (order is null)
    {
        return Results.BadRequest(new { error });
    }

    return Results.Created($"/api/orders/{order.Id}", order);
})
.WithName("CreateOrder");

app.MapGet("/api/orders/{id:int}", (int id, OrderService service) =>
{
    var order = service.GetById(id);

    if (order is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
})
.WithName("GetOrderById");

app.Run();

public partial class Program { }
