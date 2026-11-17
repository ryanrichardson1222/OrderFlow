
/*
    OrderFlow - Stored Procedures
    Database name is supplied at execution time.

    Creates application-facing and administrative
    procedures for order management.
*/

USE [$(DatabaseName)];
GO

/* 1. Retrieve all active orders */
CREATE OR ALTER PROCEDURE dbo.usp_GetAllOrders
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.OrderDate,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    WHERE o.IsDeleted = 0
    ORDER BY o.OrderId;
END;
GO

/* 2. Retrieve one active order by ID */
CREATE OR ALTER PROCEDURE dbo.usp_GetOrderById
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.OrderDate,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    WHERE o.OrderId = @OrderId
      AND o.IsDeleted = 0;
END;
GO

/* 3. Create an order for an existing user */
CREATE OR ALTER PROCEDURE dbo.usp_CreateOrder
    @UserId INT,
    @Total DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Users
        WHERE UserId = @UserId
    )
    BEGIN
        THROW 50001, 'The specified user does not exist.', 1;
    END;

    IF @Total IS NULL OR @Total < 0
    BEGIN
        THROW 50002, 'Order total must be zero or greater.', 1;
    END;

    INSERT INTO dbo.Orders (
        UserId,
        Total
    )
    VALUES (
        @UserId,
        @Total
    );

    DECLARE @NewOrderId INT = CONVERT(INT, SCOPE_IDENTITY());

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.OrderDate,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    WHERE o.OrderId = @NewOrderId;
END;
GO

/* 4. Update the status of an active order */
CREATE OR ALTER PROCEDURE dbo.usp_UpdateOrderStatus
    @OrderId INT,
    @Status NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Status = LTRIM(RTRIM(@Status));

    IF @Status IS NULL OR @Status NOT IN (
        N'Pending',
        N'Processing',
        N'Shipped',
        N'Cancelled'
    )
    BEGIN
        THROW 50004, 'The specified order status is invalid.', 1;
    END;

    UPDATE dbo.Orders
    SET Status = @Status
    WHERE OrderId = @OrderId
      AND IsDeleted = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Orders
            WHERE OrderId = @OrderId
        )
        BEGIN
            THROW 50003, 'The specified order does not exist.', 1;
        END;

        THROW 50009, 'Cannot update an order that has been deleted.', 1;
    END;

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.OrderDate,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    WHERE o.OrderId = @OrderId;
END;
GO

/* 5. Soft-delete an order */
CREATE OR ALTER PROCEDURE dbo.usp_DeleteOrder
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Orders
    SET
        IsDeleted = 1,
        DeletedAt = SYSUTCDATETIME()
    WHERE OrderId = @OrderId
      AND IsDeleted = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Orders
            WHERE OrderId = @OrderId
        )
        BEGIN
            THROW 50005, 'The specified order does not exist.', 1;
        END;

        THROW 50006, 'The specified order has already been deleted.', 1;
    END;

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    WHERE o.OrderId = @OrderId;
END;
GO

/* 6. Restore a soft-deleted order */
CREATE OR ALTER PROCEDURE dbo.usp_RestoreOrder
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Orders
    SET
        IsDeleted = 0,
        DeletedAt = NULL
    WHERE OrderId = @OrderId
      AND IsDeleted = 1;

    IF @@ROWCOUNT = 0
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Orders
            WHERE OrderId = @OrderId
        )
        BEGIN
            THROW 50007, 'The specified order does not exist.', 1;
        END;

        THROW 50008, 'The specified order is not deleted.', 1;
    END;

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    WHERE o.OrderId = @OrderId;
END;
GO

/* 7. Administrative retrieval of all orders */
CREATE OR ALTER PROCEDURE dbo.usp_GetAllOrdersIncludingDeleted
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        o.OrderId,
        o.UserId,
        u.FirstName,
        u.LastName,
        o.OrderDate,
        o.Status,
        o.Total,
        o.IsDeleted,
        o.DeletedAt
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId
    ORDER BY o.OrderId;
END;
GO
