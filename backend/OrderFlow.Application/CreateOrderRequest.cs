
namespace OrderFlow.Application;

public record CreateOrderRequest(
    string CustomerName,
    decimal Total);
