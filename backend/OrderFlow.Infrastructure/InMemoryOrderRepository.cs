using OrderFlow.Application;

namespace OrderFlow.Infrastructure;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders =
    [
    new Order(1, "Acme Corporation", "Processing", 249.99m),
new Order(2, "Northstar Logistics", "Shipped", 1249.50m),
new Order(3, "Summit Healthcare", "Pending", 89.95m)
    ];

    public Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Order> orders = _orders.AsReadOnly();

        return Task.FromResult(orders);
    }

    public Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var order = _orders.FirstOrDefault(order => order.Id == id);

        return Task.FromResult(order);
    }

    public Task<Order> AddAsync(
        string customerName,
        decimal total,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var nextId = _orders.Max(order => order.Id) + 1;

        var order = new Order(
            nextId,
            customerName,
            "Pending",
            total);

        _orders.Add(order);

        return Task.FromResult(order);
    }

}
