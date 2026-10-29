namespace OrderFlow.Application;

public class InMemoryOrderRepository : IOrderRepository
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

    public Order Add(string customerName, decimal total)
    {
        var nextId = _orders.Max(order => order.Id) + 1;

        var order = new Order(
            nextId,
            customerName,
            "Pending",
            total);

        _orders.Add(order);

        return order;
    }
}
