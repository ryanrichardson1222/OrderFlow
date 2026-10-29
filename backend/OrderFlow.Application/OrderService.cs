
namespace OrderFlow.Application;

public class OrderService
{
    private readonly List<Order> _orders =
    [
        new Order(1, "Acme Corporation", "Processing", 249.99m),
        new Order(2, "Northstar Logistics", "Shipped", 1249.50m),
        new Order(3, "Summit Healthcare", "Pending", 89.95m)
    ];

    public IReadOnlyList<Order> GetAll()
    {
        return _orders.AsReadOnly();
    }

    public Order? GetById(int id)
    {
        return _orders.FirstOrDefault(order => order.Id == id);
    }

    public Order? Create(CreateOrderRequest request, out string? error)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            error = "Customer name is required.";
            return null;
        }

        if (request.Total < 0)
        {
            error = "Order total cannot be negative.";
            return null;
        }

        var nextId = _orders.Max(order => order.Id) + 1;

        var order = new Order(
            nextId,
            request.CustomerName.Trim(),
            "Pending",
            request.Total);

        _orders.Add(order);

        error = null;
        return order;
    }
}
