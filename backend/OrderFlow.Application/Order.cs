namespace OrderFlow.Application;

public record Order(
    int Id,
    string CustomerName,
    string Status,
    decimal Total);
