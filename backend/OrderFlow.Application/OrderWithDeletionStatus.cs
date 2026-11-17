
namespace OrderFlow.Application;

public record OrderWithDeletionStatus(
    Order Order,
    DateTime OrderDate,
    bool IsDeleted,
    DateTime? DeletedAt);
