namespace OrderFlow.Application;

public record CreateOrderRequest(
    int UserId,
    decimal Total);
