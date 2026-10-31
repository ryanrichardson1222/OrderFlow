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

/* 3. All nine named constraints, including defaults */
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
SELECT COUNT(*) AS UserCount FROM dbo.Users;
SELECT COUNT(*) AS OrderCount FROM dbo.Orders;
GO

/* 5. Confirm view exists */
SELECT OBJECT_ID(N'dbo.vw_UserOrders', N'V') AS UserOrdersViewObjectId;
GO

/* 6. Joined order details */
SELECT *
FROM dbo.vw_UserOrders
ORDER BY OrderId;
GO

/* 7. Orders per user */
SELECT
    u.UserId,
    u.FirstName,
    u.LastName,
    COUNT(o.OrderId) AS OrderCount
FROM dbo.Users AS u
LEFT JOIN dbo.Orders AS o
    ON u.UserId = o.UserId
GROUP BY
    u.UserId,
    u.FirstName,
    u.LastName
ORDER BY u.UserId;
GO
