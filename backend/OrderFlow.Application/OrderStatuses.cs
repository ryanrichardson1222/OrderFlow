namespace OrderFlow.Application;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Cancelled = "Cancelled";

    public static bool TryNormalize(
        string? status,
        out string normalizedStatus)
    {
        normalizedStatus = string.Empty;

        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var normalized = status.Trim();

        if (string.Equals(normalized, Pending, StringComparison.OrdinalIgnoreCase))
        {
            normalizedStatus = Pending;
            return true;
        }

        if (string.Equals(normalized, Processing, StringComparison.OrdinalIgnoreCase))
        {
            normalizedStatus = Processing;
            return true;
        }

        if (string.Equals(normalized, Shipped, StringComparison.OrdinalIgnoreCase))
        {
            normalizedStatus = Shipped;
            return true;
        }

        if (string.Equals(normalized, Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            normalizedStatus = Cancelled;
            return true;
        }

        return false;
    }
}
