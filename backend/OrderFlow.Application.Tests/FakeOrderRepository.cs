using OrderFlow.Application;

namespace OrderFlow.Application.Tests;

public class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<int, string> _users = new()
    {
        { 1, "Ryan Richardson" },
        { 2, "Alex Morgan" },
        { 3, "Jordan Blake" }
    };

    private readonly List<Order> _orders =
    [
        new Order(1, 1, "Ryan Richardson", "Processing", 249.99m),
        new Order(2, 1, "Ryan Richardson", "Pending", 89.95m),
        new Order(3, 2, "Alex Morgan", "Shipped", 1249.50m),
        new Order(4, 3, "Jordan Blake", "Pending", 499.00m)
    ];

    public int AddCallCount { get; private set; }

    public int LastUserId { get; private set; }

    public decimal LastTotal { get; private set; }

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

    public Task<bool> UserExistsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_users.ContainsKey(userId));
    }

    public Task<Order> AddAsync(
        int userId,
        decimal total,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_users.TryGetValue(userId, out var customerName))
        {
            throw new InvalidOperationException(
                $"User {userId} does not exist.");
        }

        AddCallCount++;
        LastUserId = userId;
        LastTotal = total;

        var nextId = _orders.Max(order => order.Id) + 1;

        var order = new Order(
            nextId,
            userId,
            customerName,
            "Pending",
            total);

        _orders.Add(order);

        return Task.FromResult(order);
    }
}
