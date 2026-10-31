/*
    OrderFlow - User Orders View
    Database name is supplied at execution time.
    Use an isolated database for validation and testing.

    Provides order details alongside associated user information.
*/

USE [$(DatabaseName)];
GO

CREATE OR ALTER VIEW dbo.vw_UserOrders
AS
    SELECT
        o.OrderId,
        u.UserId,
        u.FirstName,
        u.LastName,
        u.Email,
        o.OrderDate,
        o.Status,
        o.Total
    FROM dbo.Orders AS o
    INNER JOIN dbo.Users AS u
        ON o.UserId = u.UserId;
GO
