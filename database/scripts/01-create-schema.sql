/*
    OrderFlow - Database Schema
    Database name is supplied at execution time.
    Use an isolated database for validation and testing.

    Creates the database and core Users / Orders tables.
*/

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    DECLARE @CreateDatabaseSql NVARCHAR(MAX);

    SET @CreateDatabaseSql =
        N'CREATE DATABASE ' + QUOTENAME(N'$(DatabaseName)');

    EXEC sys.sp_executesql @CreateDatabaseSql;
END;
GO

USE [$(DatabaseName)];
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
        IsDeleted BIT NOT NULL
            CONSTRAINT DF_Orders_IsDeleted DEFAULT (0),
        DeletedAt DATETIME2(0) NULL,

        CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
        CONSTRAINT FK_Orders_Users
            FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_Orders_Total CHECK (Total >= 0),
        CONSTRAINT CK_Orders_Status
            CHECK (Status IN (N'Pending', N'Processing', N'Shipped', N'Cancelled'))
    );
END;
GO
