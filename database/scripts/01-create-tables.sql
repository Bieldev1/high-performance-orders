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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] bigint NOT NULL IDENTITY,
        [Nome] varchar(150) NOT NULL,
        [Email] varchar(200) NOT NULL,
        [DataCadastro] datetime2 NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    CREATE TABLE [Pedidos] (
        [Id] bigint NOT NULL IDENTITY,
        [ClienteId] bigint NOT NULL,
        [Status] int NOT NULL,
        [ValorTotal] decimal(18,2) NOT NULL,
        [DataPedido] datetime2 NOT NULL,
        CONSTRAINT [PK_Pedidos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    CREATE TABLE [ItensPedido] (
        [Id] bigint NOT NULL IDENTITY,
        [PedidoId] bigint NOT NULL,
        [NomeProduto] varchar(200) NOT NULL,
        [Quantidade] int NOT NULL,
        [PrecoUnitario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ItensPedido] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItensPedido_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItensPedido_PedidoId] ON [ItensPedido] ([PedidoId]) INCLUDE ([NomeProduto], [Quantidade]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pedidos_Covering] ON [Pedidos] ([Status], [ValorTotal]) INCLUDE ([DataPedido], [ClienteId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pedidos_Status_ValorTotal_DataPedido] ON [Pedidos] ([Status], [ValorTotal], [DataPedido] DESC) INCLUDE ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904192102_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904192102_InitialCreate', N'8.0.10');
END;
GO

COMMIT;
GO

