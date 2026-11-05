/*
    OrderFlow - Database Verification
    Database name is supplied at execution time.
    Use an isolated database for validation and testing.

    Checks database status, tables, constraints, seed data, and view output.
*/

USE [$(DatabaseName)];
GO

/* 1. Database status */
SELECT
    name,
    state_desc,
    recovery_model_desc
FROM sys.databases
WHERE name = N'$(DatabaseName)';
GO

/* 2. Tables */
SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

/* 3. Named constraints */
SELECT
    t.name AS TableName,
    o.name AS ConstraintName,
    o.type_desc AS ConstraintType
FROM sys.objects AS o
INNER JOIN sys.tables AS t
    ON o.parent_object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID(N'dbo')
  AND o.type IN ('PK', 'UQ', 'F', 'C', 'D')
ORDER BY t.name, o.name;
GO

/* 4. User and order counts */
SELECT COUNT(*) AS UserCount
FROM dbo.Users;

SELECT
    COUNT(*) AS TotalOrderCount,
    COALESCE(SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END), 0) AS ActiveOrderCount,
    COALESCE(SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END), 0) AS DeletedOrderCount
FROM dbo.Orders;
GO

/* 5. Confirm soft-delete columns exist */
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'Orders'
  AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt')
ORDER BY ORDINAL_POSITION;
GO

/* 6. Confirm view exists */
SELECT
    OBJECT_ID(N'dbo.vw_UserOrders', N'V') AS UserOrdersViewObjectId;
GO

/* 7. Confirm stored procedures exist */
SELECT
    name AS ProcedureName,
    create_date AS CreatedDate,
    modify_date AS ModifiedDate
FROM sys.procedures
WHERE schema_id = SCHEMA_ID(N'dbo')
  AND name IN (
      N'usp_GetAllOrders',
      N'usp_GetOrderById',
      N'usp_CreateOrder',
      N'usp_UpdateOrderStatus',
      N'usp_DeleteOrder',
      N'usp_RestoreOrder',
      N'usp_GetAllOrdersIncludingDeleted'
  )
ORDER BY name;
GO

/* 8. Joined active order details */
SELECT *
FROM dbo.vw_UserOrders
ORDER BY OrderId;
GO

/* 9. Active orders per user */
SELECT
    u.UserId,
    u.FirstName,
    u.LastName,
    COUNT(o.OrderId) AS ActiveOrderCount
FROM dbo.Users AS u
LEFT JOIN dbo.Orders AS o
    ON u.UserId = o.UserId
   AND o.IsDeleted = 0
GROUP BY
    u.UserId,
    u.FirstName,
    u.LastName
ORDER BY u.UserId;
GO
