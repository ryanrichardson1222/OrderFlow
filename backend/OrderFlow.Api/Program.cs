using OrderFlow.Application;
using OrderFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<OrderService>();

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

app.MapPut("/api/orders/{id:int}/status", async (
    int id,
    UpdateOrderStatusRequest request,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.UpdateStatusAsync(
        id,
        request,
        cancellationToken);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Order);
    }

    return result.ErrorCode switch
    {
        UpdateOrderStatusErrorCode.InvalidOrderId =>
            Results.BadRequest(new { error = result.Error }),

        UpdateOrderStatusErrorCode.InvalidStatus =>
            Results.BadRequest(new { error = result.Error }),

        UpdateOrderStatusErrorCode.OrderNotFound =>
            Results.NotFound(new { error = result.Error }),

        UpdateOrderStatusErrorCode.OrderDeleted =>
            Results.Conflict(new { error = result.Error }),

        UpdateOrderStatusErrorCode.UpdateFailed =>
            Results.Problem(
                statusCode: 500,
                title: "Order update failed",
                detail: result.Error),

        _ => Results.Problem(
            statusCode: 500,
            title: "Unexpected order update error")
    };
})
.WithName("UpdateOrderStatus");

app.MapDelete("/api/orders/{id:int}", async (
    int id,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.DeleteAsync(
        id,
        cancellationToken);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Order);
    }

    return result.ErrorCode switch
    {
        OrderMutationErrorCode.InvalidOrderId =>
            Results.BadRequest(new { error = "A valid order ID is required." }),

        OrderMutationErrorCode.OrderNotFound =>
            Results.NotFound(new { error = "The specified order does not exist." }),

        OrderMutationErrorCode.AlreadyDeleted =>
            Results.Conflict(new { error = "The specified order has already been deleted." }),

        _ => Results.Problem(
            statusCode: 500,
            title: "Order deletion failed")
    };
})
.WithName("DeleteOrder");

app.MapPost("/api/orders/{id:int}/restore", async (
    int id,
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.RestoreAsync(
        id,
        cancellationToken);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Order);
    }

    return result.ErrorCode switch
    {
        OrderMutationErrorCode.InvalidOrderId =>
            Results.BadRequest(new { error = "A valid order ID is required." }),

        OrderMutationErrorCode.OrderNotFound =>
            Results.NotFound(new { error = "The specified order does not exist." }),

        OrderMutationErrorCode.NotDeleted =>
            Results.Conflict(new { error = "The specified order is not deleted." }),

        _ => Results.Problem(
            statusCode: 500,
            title: "Order restoration failed")
    };
})
.WithName("RestoreOrder");

app.MapGet("/api/orders/admin/all", async (
    OrderService service,
    CancellationToken cancellationToken) =>
{
    var orders = await service.GetAllIncludingDeletedAsync(
        cancellationToken);

    return Results.Ok(orders);
})
.WithName("GetAllOrdersIncludingDeleted");

app.Run();

public partial class Program { }
