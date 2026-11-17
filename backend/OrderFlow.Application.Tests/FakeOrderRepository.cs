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

    private readonly HashSet<int> _deletedOrderIds = [];

    private readonly Dictionary<int, DateTime> _orderDates;

    public FakeOrderRepository()
    {
        _orderDates = _orders.ToDictionary(
            order => order.Id,
            _ => DateTime.UtcNow);
    }

    private readonly Dictionary<int, DateTime> _deletedAt = new();

    public int AddCallCount { get; private set; }

    public int LastUserId { get; private set; }

    public decimal LastTotal { get; private set; }

    public int UpdateStatusCallCount { get; private set; }

    public Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Order> orders = _orders
            .Where(order => !_deletedOrderIds.Contains(order.Id))
            .ToList()
            .AsReadOnly();

        return Task.FromResult(orders);
    }

    public Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var order = _orders.FirstOrDefault(
            order => order.Id == id && !_deletedOrderIds.Contains(order.Id));

        return Task.FromResult(order);
    }


    public Task<OrderMutationResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var order = _orders.FirstOrDefault(order => order.Id == id);

        if (order is null)
        {
            return Task.FromResult(new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderNotFound));
        }

        if (_deletedOrderIds.Contains(id))
        {
            return Task.FromResult(new OrderMutationResult(
                null,
                OrderMutationErrorCode.AlreadyDeleted));
        }

        _deletedOrderIds.Add(id);
        _deletedAt[id] = DateTime.UtcNow;

        return Task.FromResult(new OrderMutationResult(order, null));
    }


    public Task<OrderMutationResult> RestoreAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var order = _orders.FirstOrDefault(order => order.Id == id);

        if (order is null)
        {
            return Task.FromResult(new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderNotFound));
        }

        if (!_deletedOrderIds.Contains(id))
        {
            return Task.FromResult(new OrderMutationResult(
                null,
                OrderMutationErrorCode.NotDeleted));
        }

        _deletedOrderIds.Remove(id);
        _deletedAt.Remove(id);

        return Task.FromResult(new OrderMutationResult(order, null));
    }


    public Task<IReadOnlyList<OrderWithDeletionStatus>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<OrderWithDeletionStatus> orders = _orders
            .Select(order => new OrderWithDeletionStatus(
                order,
                _orderDates[order.Id],
                _deletedOrderIds.Contains(order.Id),
                _deletedAt.TryGetValue(order.Id, out var deletedAt)
                    ? deletedAt
                    : (DateTime?)null))
            .ToList();

        return Task.FromResult(orders);
    }


    public Task<OrderMutationResult> UpdateStatusAsync(
        int id,
        string status,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UpdateStatusCallCount++;

        var index = _orders.FindIndex(order => order.Id == id);

        if (index < 0)
        {
            return Task.FromResult(new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderNotFound));
        }

        if (_deletedOrderIds.Contains(id))
        {
            return Task.FromResult(new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderDeleted));
        }

        var updatedOrder = _orders[index] with
        {
            Status = status
        };

        _orders[index] = updatedOrder;

        return Task.FromResult(new OrderMutationResult(updatedOrder, null));
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
        _orderDates[order.Id] = DateTime.UtcNow;
        return Task.FromResult(order);
    }
}
