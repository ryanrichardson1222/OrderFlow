namespace OrderFlow.Application;

public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<OrderMutationResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.InvalidOrderId);
        }

        return await _repository.DeleteAsync(
            id,
            cancellationToken);
    }

    public async Task<OrderMutationResult> RestoreAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.InvalidOrderId);
        }

        return await _repository.RestoreAsync(
            id,
            cancellationToken);
    }

    public Task<IReadOnlyList<OrderWithDeletionStatus>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAllIncludingDeletedAsync(cancellationToken);
    }

    public async Task<CreateOrderResult> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
        {
            return new CreateOrderResult(
                null,
                "A valid user ID is required.");
        }

        if (request.Total < 0)
        {
            return new CreateOrderResult(
                null,
                "Order total cannot be negative.");
        }

        var userExists = await _repository.UserExistsAsync(
            request.UserId,
            cancellationToken);

        if (!userExists)
        {
            return new CreateOrderResult(
                null,
                "The specified user does not exist.");
        }

        var order = await _repository.AddAsync(
            request.UserId,
            request.Total,
            cancellationToken);

        return new CreateOrderResult(order, null);
    }

    public async Task<UpdateOrderStatusResult> UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return new UpdateOrderStatusResult(
                null,
                "A valid order ID is required.",
                UpdateOrderStatusErrorCode.InvalidOrderId);
        }

        if (!OrderStatuses.TryNormalize(request.Status, out var normalizedStatus))
        {
            return new UpdateOrderStatusResult(
                null,
                "The specified order status is invalid.",
                UpdateOrderStatusErrorCode.InvalidStatus);
        }

        var updateResult = await _repository.UpdateStatusAsync(
            id,
            normalizedStatus,
            cancellationToken);

        if (!updateResult.IsSuccess)
        {
            return updateResult.ErrorCode switch
            {
                OrderMutationErrorCode.OrderNotFound =>
                    new UpdateOrderStatusResult(
                        null,
                        "The specified order does not exist.",
                        UpdateOrderStatusErrorCode.OrderNotFound),

                OrderMutationErrorCode.OrderDeleted =>
                    new UpdateOrderStatusResult(
                        null,
                        "Cannot update an order that has been deleted.",
                        UpdateOrderStatusErrorCode.OrderDeleted),

                _ => new UpdateOrderStatusResult(
                    null,
                    "The order could not be updated.",
                    UpdateOrderStatusErrorCode.UpdateFailed)
            };
        }

        return new UpdateOrderStatusResult(updateResult.Order, null, null);

    }
}
