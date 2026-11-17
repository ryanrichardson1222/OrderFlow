namespace OrderFlow.Application;

public record UpdateOrderStatusResult(
    Order? Order,
    string? Error,
    UpdateOrderStatusErrorCode? ErrorCode)
{
    public bool IsSuccess => Order is not null && Error is null;
}

public enum UpdateOrderStatusErrorCode
{
    InvalidOrderId,
    InvalidStatus,
    OrderNotFound,
    OrderDeleted,
    UpdateFailed
}
