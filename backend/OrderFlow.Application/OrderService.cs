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
}
