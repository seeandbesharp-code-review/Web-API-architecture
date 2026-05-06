-- =============================================
-- WebApiShop Database Creation Script
-- =============================================

USE master;
GO

-- Create the database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Shop')
BEGIN
    CREATE DATABASE Shop;
END
GO

USE Shop;
GO

-- =============================================
-- Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Category_id   INT           IDENTITY(1,1) NOT NULL,
        Category_name NVARCHAR(50)  NULL,
        CONSTRAINT PK_Categories PRIMARY KEY (Category_id)
    );
END
GO

-- =============================================
-- Users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id         INT           IDENTITY(1,1) NOT NULL,
        Email      NVARCHAR(50)  NOT NULL,
        First_name NVARCHAR(50)  NULL,
        Last_name  NVARCHAR(50)  NULL,
        Password   NVARCHAR(20)  NOT NULL,
        CONSTRAINT PK_Users_1 PRIMARY KEY (Id)
    );
END
GO

-- =============================================
-- Products
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Product_id   INT           IDENTITY(1,1) NOT NULL,
        Product_name NVARCHAR(50)  NOT NULL,
        Price        MONEY         NOT NULL,
        Category_id  INT           NOT NULL,
        Description  NVARCHAR(100) NULL,
        CONSTRAINT PK_Products PRIMARY KEY (Product_id),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (Category_id)
            REFERENCES Categories (Category_id) ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    CREATE INDEX IX_Products_Category_id ON Products (Category_id);
END
GO

-- =============================================
-- Orders
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Order_id   INT     IDENTITY(1,1) NOT NULL,
        Order_date DATE    NULL,
        Order_sum  INT     NULL,
        User_id    INT     NOT NULL,
        CONSTRAINT PK_Orders PRIMARY KEY (Order_id),
        CONSTRAINT FK_Orders_Users FOREIGN KEY (User_id)
            REFERENCES Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    CREATE INDEX IX_Orders_User_id ON Orders (User_id);
END
GO

-- =============================================
-- Order_item
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Order_item')
BEGIN
    CREATE TABLE Order_item (
        Order_item_id INT NOT NULL IDENTITY(1,1),
        Product_id    INT NOT NULL,
        Order_id      INT NOT NULL,
        Quantity      INT NOT NULL,
        CONSTRAINT PK_Order_item PRIMARY KEY (Order_item_id),
        CONSTRAINT FK_OrderItem_Orders FOREIGN KEY (Order_id)
            REFERENCES Orders (Order_id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_OrderItem_Products FOREIGN KEY (Product_id)
            REFERENCES Products (Product_id) ON DELETE NO ACTION ON UPDATE NO ACTION
    );

    CREATE INDEX IX_Order_item_Order_id   ON Order_item (Order_id);
    CREATE INDEX IX_Order_item_Product_id ON Order_item (Product_id);
END
GO

-- =============================================
-- Rating
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rating')
BEGIN
    CREATE TABLE Rating (
        RATING_ID   INT           IDENTITY(1,1) NOT NULL,
        HOST        NVARCHAR(50)  NULL,
        METHOD      NCHAR(10)     NULL,
        PATH        NVARCHAR(50)  NULL,
        REFERER     NVARCHAR(100) NULL,
        USER_AGENT  NVARCHAR(MAX) NULL,
        Record_Date DATETIME      NULL,
        CONSTRAINT PK_RATING PRIMARY KEY (RATING_ID)
    );
END
GO

-- =============================================
-- Sample seed data (optional)
-- =============================================

-- Categories
INSERT INTO Categories (Category_name) VALUES
    ('Electronics'),
    ('Clothing'),
    ('Books'),
    ('Home & Garden'),
    ('Sports');
GO

-- Users (passwords meet minimum zxcvbn strength 2)
INSERT INTO Users (Email, First_name, Last_name, Password) VALUES
    ('admin@shop.com',   'Admin',  'User',   'Admin@1234!'),
    ('alice@shop.com',   'Alice',  'Smith',  'Alice@5678!'),
    ('bob@shop.com',     'Bob',    'Jones',  'Bobby#9012!');
GO

-- Products
INSERT INTO Products (Product_name, Price, Category_id, Description) VALUES
    ('Laptop Pro 15',   1299.99, 1, 'High-performance laptop'),
    ('Wireless Mouse',    29.99, 1, 'Ergonomic wireless mouse'),
    ('T-Shirt XL',        19.99, 2, 'Cotton t-shirt extra large'),
    ('Clean Code',        39.99, 3, 'Software craftsmanship book'),
    ('Garden Hose 10m',   24.99, 4, '10-meter flexible garden hose');
GO
