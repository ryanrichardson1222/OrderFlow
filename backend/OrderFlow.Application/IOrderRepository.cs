namespace OrderFlow.Application;

public interface IOrderRepository
{
    IReadOnlyList<Order> GetAll();

    Order? GetById(int id);

    Order Add(string customerName, decimal total);
}
