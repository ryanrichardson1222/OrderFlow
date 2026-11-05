namespace OrderFlow.Application;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Order> AddAsync(
        int userId,
        decimal total,
        CancellationToken cancellationToken = default);
}
