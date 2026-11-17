namespace OrderFlow.Application;

public record CreateOrderResult(Order? Order, string? Error)
{
    public bool IsSuccess => Order is not null && Error is null;
}