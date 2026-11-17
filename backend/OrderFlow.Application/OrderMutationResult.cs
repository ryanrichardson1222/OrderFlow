
namespace OrderFlow.Application;

public record OrderMutationResult(
    Order? Order,
    OrderMutationErrorCode? ErrorCode)
{
    public bool IsSuccess => Order is not null && ErrorCode is null;
}

public enum OrderMutationErrorCode
{
    InvalidOrderId,
    OrderNotFound,
    OrderDeleted,
    AlreadyDeleted,
    NotDeleted
}
