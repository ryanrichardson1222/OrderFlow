namespace OrderFlow.Application;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Order> AddAsync(
        string customerName,
        decimal total,
        CancellationToken cancellationToken = default);
}
