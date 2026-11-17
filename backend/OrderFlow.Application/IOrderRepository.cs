namespace OrderFlow.Application;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OrderMutationResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OrderMutationResult> RestoreAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderWithDeletionStatus>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default);

    Task<OrderMutationResult> UpdateStatusAsync(
        int id,
        string status,
        CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Order> AddAsync(
        int userId,
        decimal total,
        CancellationToken cancellationToken = default);

}
