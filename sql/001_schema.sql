-- ============================================================
-- 001_schema.sql
-- Data Access Benchmark Lab
-- ============================================================

IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
GO

CREATE TABLE dbo.Orders
(
    Id            INT IDENTITY(1,1) NOT NULL,
    CustomerName  NVARCHAR(100)     NOT NULL,
    CustomerEmail NVARCHAR(150)     NOT NULL,
    Status        NVARCHAR(20)      NOT NULL,      -- Pending / Shipped / Delivered / Cancelled
    TotalAmount   DECIMAL(18,2)     NOT NULL,
    CreatedAt     DATETIME2(3)      NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE TABLE dbo.OrderItems
(
    Id          INT IDENTITY(1,1) NOT NULL,
    OrderId     INT               NOT NULL,
    ProductName NVARCHAR(150)     NOT NULL,
    Quantity    INT               NOT NULL,
    UnitPrice   DECIMAL(18,2)     NOT NULL,

    CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId)
        REFERENCES dbo.Orders (Id)
        ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId
    ON dbo.OrderItems (OrderId);
GO

CREATE NONCLUSTERED INDEX IX_Orders_Status
    ON dbo.Orders (Status);
GO
