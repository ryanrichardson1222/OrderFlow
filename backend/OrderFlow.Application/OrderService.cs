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
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return new CreateOrderResult(
                null,
                "Customer name is required.");
        }

        if (request.Total < 0)
        {
            return new CreateOrderResult(
                null,
                "Order total cannot be negative.");
        }

        var order = await _repository.AddAsync(
            request.CustomerName.Trim(),
            request.Total,
            cancellationToken);

        return new CreateOrderResult(order, null);
    }
}
