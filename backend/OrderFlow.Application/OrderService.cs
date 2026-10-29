namespace OrderFlow.Application;

public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Order> GetAll()
    {
        return _repository.GetAll();
    }

    public Order? GetById(int id)
    {
        return _repository.GetById(id);
    }

    public Order? Create(CreateOrderRequest request, out string? error)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            error = "Customer name is required.";
            return null;
        }

        if (request.Total < 0)
        {
            error = "Order total cannot be negative.";
            return null;
        }

        var order = _repository.Add(
            request.CustomerName.Trim(),
            request.Total);

        error = null;
        return order;
    }
}
