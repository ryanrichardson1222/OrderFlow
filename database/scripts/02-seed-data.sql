/*
    OrderFlow - Synthetic Seed Data
    Database name is supplied at execution time.
    Use an isolated database for validation and testing.

    Inserts three sample users and four sample orders.
    Safe to rerun without duplicating these sample records.
*/

USE [$(DatabaseName)];
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/* Users */
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'ryan@example.test')
BEGIN
    INSERT INTO dbo.Users (FirstName, LastName, Email)
    VALUES (N'Ryan', N'Richardson', N'ryan@example.test');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'alex@example.test')
BEGIN
    INSERT INTO dbo.Users (FirstName, LastName, Email)
    VALUES (N'Alex', N'Morgan', N'alex@example.test');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'jordan@example.test')
BEGIN
    INSERT INTO dbo.Users (FirstName, LastName, Email)
    VALUES (N'Jordan', N'Blake', N'jordan@example.test');
END;

/* Orders */
DECLARE @RyanUserId INT =
    (SELECT UserId FROM dbo.Users WHERE Email = N'ryan@example.test');

DECLARE @AlexUserId INT =
    (SELECT UserId FROM dbo.Users WHERE Email = N'alex@example.test');

DECLARE @JordanUserId INT =
    (SELECT UserId FROM dbo.Users WHERE Email = N'jordan@example.test');

IF NOT EXISTS (
    SELECT 1 FROM dbo.Orders
    WHERE UserId = @RyanUserId AND Status = N'Processing' AND Total = 249.99
)
BEGIN
    INSERT INTO dbo.Orders (UserId, Status, Total)
    VALUES (@RyanUserId, N'Processing', 249.99);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.Orders
    WHERE UserId = @RyanUserId AND Status = N'Pending' AND Total = 89.95
)
BEGIN
    INSERT INTO dbo.Orders (UserId, Status, Total)
    VALUES (@RyanUserId, N'Pending', 89.95);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.Orders
    WHERE UserId = @AlexUserId AND Status = N'Shipped' AND Total = 1249.50
)
BEGIN
    INSERT INTO dbo.Orders (UserId, Status, Total)
    VALUES (@AlexUserId, N'Shipped', 1249.50);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.Orders
    WHERE UserId = @JordanUserId AND Status = N'Pending' AND Total = 499.00
)
BEGIN
    INSERT INTO dbo.Orders (UserId, Status, Total)
    VALUES (@JordanUserId, N'Pending', 499.00);
END;

COMMIT TRANSACTION;
GO
