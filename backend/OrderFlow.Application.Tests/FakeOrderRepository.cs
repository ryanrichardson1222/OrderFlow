using OrderFlow.Application;

namespace OrderFlow.Application.Tests;

public class FakeOrderRepository : IOrderRepository
{
    public List<Order> Orders { get; } =
    [
        new Order(1, "Acme Corporation", "Processing", 249.99m),
        new Order(2, "Northstar Logistics", "Shipped", 1249.50m),
        new Order(3, "Summit Healthcare", "Pending", 89.95m)
    ];

    public int AddCallCount { get; private set; }

    public string? LastCustomerName { get; private set; }

    public decimal? LastTotal { get; private set; }

    public IReadOnlyList<Order> GetAll()
    {
        return Orders.AsReadOnly();
    }

    public Order? GetById(int id)
    {
        return Orders.FirstOrDefault(order => order.Id == id);
    }

    public Order Add(string customerName, decimal total)
    {
        AddCallCount++;
        LastCustomerName = customerName;
        LastTotal = total;

        var nextId = Orders.Max(order => order.Id) + 1;
        var order = new Order(nextId, customerName, "Pending", total);

        Orders.Add(order);

        return order;
    }
}
