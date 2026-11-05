namespace OrderFlow.Application;

public record Order(
    int Id,
    int UserId,
    string CustomerName,
    string Status,
    decimal Total);
