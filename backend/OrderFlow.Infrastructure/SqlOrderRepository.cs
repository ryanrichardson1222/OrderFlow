using System.Data;
using Dapper;
using OrderFlow.Application;
using Microsoft.Data.SqlClient;

namespace OrderFlow.Infrastructure;

public sealed class SqlOrderRepository : IOrderRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SqlOrderRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.usp_GetAllOrders",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var data = await connection.QueryAsync<OrderData>(command);

        return data.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<OrderWithDeletionStatus>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.usp_GetAllOrdersIncludingDeleted",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var data = await connection.QueryAsync<OrderData>(command);

        return data.Select(item => new OrderWithDeletionStatus(
            Map(item),
            item.OrderDate,
            item.IsDeleted,
            item.DeletedAt)).ToList();
    }

    public async Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.usp_GetOrderById",
            parameters: new { OrderId = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var data = await connection.QuerySingleOrDefaultAsync<OrderData>(command);

        return data is null ? null : Map(data);
    }

    public async Task<OrderMutationResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.usp_DeleteOrder",
                parameters: new { OrderId = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var data = await connection.QuerySingleAsync<OrderData>(command);

            return new OrderMutationResult(Map(data), null);
        }
        catch (SqlException ex) when (ex.Number == 50005)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderNotFound);
        }
        catch (SqlException ex) when (ex.Number == 50006)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.AlreadyDeleted);
        }
    }

    public async Task<OrderMutationResult> RestoreAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.usp_RestoreOrder",
                parameters: new { OrderId = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var data = await connection.QuerySingleAsync<OrderData>(command);

            return new OrderMutationResult(Map(data), null);
        }
        catch (SqlException ex) when (ex.Number == 50007)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderNotFound);
        }
        catch (SqlException ex) when (ex.Number == 50008)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.NotDeleted);
        }
    }

    public async Task<OrderMutationResult> UpdateStatusAsync(
        int id,
        string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var command = new CommandDefinition(
                commandText: "dbo.usp_UpdateOrderStatus",
                parameters: new
                {
                    OrderId = id,
                    Status = status
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var data = await connection.QuerySingleAsync<OrderData>(command);

            return new OrderMutationResult(Map(data), null);
        }
        catch (SqlException ex) when (ex.Number == 50003)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderNotFound);
        }
        catch (SqlException ex) when (ex.Number == 50009)
        {
            return new OrderMutationResult(
                null,
                OrderMutationErrorCode.OrderDeleted);
        }
    }

    public async Task<bool> UserExistsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Users
                    WHERE UserId = @UserId
                ) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;
                """,
            parameters: new { UserId = userId },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<bool>(command);
    }

    public async Task<Order> AddAsync(
        int userId,
        decimal total,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "dbo.usp_CreateOrder",
            parameters: new
            {
                UserId = userId,
                Total = total
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var data = await connection.QuerySingleAsync<OrderData>(command);

        return Map(data);
    }

    private static Order Map(OrderData data)
    {
        return new Order(
            data.OrderId,
            data.UserId,
            $"{data.FirstName} {data.LastName}",
            data.Status,
            data.Total);
    }
}
