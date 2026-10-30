/*
    OrderFlow - Database Schema
    Target database: FullStackLearning
    SQL Server 2022

    Creates the database and core Users / Orders tables.
*/

IF DB_ID(N'FullStackLearning') IS NULL
BEGIN
    CREATE DATABASE [FullStackLearning];
END;
GO

USE [FullStackLearning];
GO

/* Users */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId INT IDENTITY(1,1) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Users PRIMARY KEY (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END;
GO

/* Orders */
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        OrderDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Orders_OrderDate DEFAULT SYSUTCDATETIME(),
        Status NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Orders_Status DEFAULT N'Pending',
        Total DECIMAL(18,2) NOT NULL,

        CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
        CONSTRAINT FK_Orders_Users
            FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_Orders_Total CHECK (Total >= 0),
        CONSTRAINT CK_Orders_Status
            CHECK (Status IN (N'Pending', N'Processing', N'Shipped', N'Cancelled'))
    );
END;
GO
