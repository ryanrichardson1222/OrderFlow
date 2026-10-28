
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Temporary in-memory order data.
// This data will reset whenever the application restarts.
var orders = new List<Order>
{
    new Order(1, "Acme Corporation", "Processing", 249.99m),
    new Order(2, "Northstar Logistics", "Shipped", 1249.50m),
    new Order(3, "Summit Healthcare", "Pending", 89.95m)
};

// GET /api/orders
// Returns all orders as JSON.
app.MapGet("/api/orders", () =>
{
    return Results.Ok(orders);
})
.WithName("GetOrders");

// POST /api/orders
// Validates the request and creates a new order.
app.MapPost("/api/orders", (CreateOrderRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerName))
    {
        return Results.BadRequest(new
        {
            error = "Customer name is required."
        });
    }

    if (request.Total < 0)
    {
        return Results.BadRequest(new
        {
            error = "Order total cannot be negative."
        });
    }

    var nextId = orders.Max(order => order.Id) + 1;

    var newOrder = new Order(
        nextId,
        request.CustomerName.Trim(),
        "Pending",
        request.Total
    );

    orders.Add(newOrder);

    return Results.Created(
        $"/api/orders/{newOrder.Id}",
        newOrder
    );
})
.WithName("CreateOrder");

// GET /api/orders/{id}
// Returns one order by ID, or 404 if it doesn't exist.
app.MapGet("/api/orders/{id:int}", (int id) =>
{
    var order = orders.FirstOrDefault(order => order.Id == id);

    if (order is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
})
.WithName("GetOrderById");

app.Run();

record Order(int Id, string CustomerName, string Status, decimal Total);
record CreateOrderRequest(string CustomerName, decimal Total);
