/*
    OrderFlow - User Orders View
    Target database: FullStackLearning

    Provides order details alongside associated user information.
*/

USE [FullStackLearning];
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
