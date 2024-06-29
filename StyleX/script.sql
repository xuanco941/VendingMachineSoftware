IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Account] (
    [AccountID] int NOT NULL IDENTITY,
    [FullName] nvarchar(50) NULL,
    [Email] nvarchar(50) NOT NULL,
    [Password] nvarchar(50) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [Address] nvarchar(300) NULL,
    [isActive] bit NOT NULL,
    [keyActive] nvarchar(max) NULL,
    [Avatar] nvarchar(max) NULL,
    [NumberPlayGame] int NULL,
    [Role] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY ([AccountID])
);
GO

CREATE TABLE [Order] (
    [OrderID] int NOT NULL IDENTITY,
    [BasePrice] float NOT NULL,
    [NetPrice] float NOT NULL,
    [Status] int NOT NULL,
    [CreateAt] datetime2 NOT NULL,
    [UpdateAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY ([OrderID])
);
GO

CREATE TABLE [Product] (
    [ProductID] int NOT NULL IDENTITY,
    [ModelUrl] nvarchar(max) NOT NULL,
    [PosterUrl] nvarchar(max) NOT NULL,
    [PosterDesignUrl1] nvarchar(max) NULL,
    [PosterDesignUrl2] nvarchar(max) NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] float NOT NULL,
    [Sale] float NOT NULL,
    [Status] bit NOT NULL,
    [CreateAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY ([ProductID])
);
GO

CREATE TABLE [OrderProduct] (
    [CartItemID] int NOT NULL IDENTITY,
    [Amount] int NOT NULL,
    [Sale] float NOT NULL,
    [Price] float NOT NULL,
    [OrderID] int NOT NULL,
    [ProductID] int NOT NULL,
    CONSTRAINT [PK_OrderProduct] PRIMARY KEY ([CartItemID]),
    CONSTRAINT [FK_OrderProduct_Order_OrderID] FOREIGN KEY ([OrderID]) REFERENCES [Order] ([OrderID]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderProduct_Product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Product] ([ProductID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Account_Email] ON [Account] ([Email]);
GO

CREATE INDEX [IX_OrderProduct_OrderID] ON [OrderProduct] ([OrderID]);
GO

CREATE INDEX [IX_OrderProduct_ProductID] ON [OrderProduct] ([ProductID]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240623073217_v1', N'6.0.25');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Product]') AND [c].[name] = N'PosterDesignUrl1');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Product] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Product] DROP COLUMN [PosterDesignUrl1];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Product]') AND [c].[name] = N'PosterDesignUrl2');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Product] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Product] DROP COLUMN [PosterDesignUrl2];
GO

ALTER TABLE [Product] ADD [NumberAvailable] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240627043656_v2', N'6.0.25');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [OrderProduct];
GO

ALTER TABLE [Order] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240628095654_v3', N'6.0.25');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Order]') AND [c].[name] = N'Description');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Order] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Order] ALTER COLUMN [Description] nvarchar(max) NULL;
GO

ALTER TABLE [Order] ADD [OrderDetail] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240629140651_v4', N'6.0.25');
GO

COMMIT;
GO

