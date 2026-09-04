-- Versão reduzida de 02-seed-data.sql, só para os testes de integração no CI.
-- Mesma lógica set-based, mas com volume pequeno (50 clientes, 500 pedidos, ~1000 itens)
-- para o pipeline não gastar tempo gerando 500k linhas só para validar que os endpoints funcionam.
-- Para benchmarks reais de performance, use 02-seed-data.sql (volume completo).

USE HighPerformanceOrders;
GO

SET NOCOUNT ON;

TRUNCATE TABLE ItensPedido;
DELETE FROM Pedidos;
DBCC CHECKIDENT ('Pedidos', RESEED, 0);
TRUNCATE TABLE Clientes;
GO

IF OBJECT_ID('tempdb..#Numbers') IS NOT NULL DROP TABLE #Numbers;

WITH L0 AS (SELECT 1 AS c UNION ALL SELECT 1),
     L1 AS (SELECT 1 AS c FROM L0 A CROSS JOIN L0 B),
     L2 AS (SELECT 1 AS c FROM L1 A CROSS JOIN L1 B),
     L3 AS (SELECT 1 AS c FROM L2 A CROSS JOIN L2 B),
     L4 AS (SELECT 1 AS c FROM L3 A CROSS JOIN L3 B),
     Nums AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM L4)
SELECT TOP (1000) n
INTO #Numbers
FROM Nums
ORDER BY n;

CREATE UNIQUE CLUSTERED INDEX IX_Numbers_n ON #Numbers (n);
GO

INSERT INTO Clientes (Nome, Email, DataCadastro)
SELECT
    CONCAT('Cliente ', n),
    CONCAT('cliente', n, '@teste.com'),
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 1460, SYSUTCDATETIME())
FROM #Numbers
WHERE n <= 50;
GO

DECLARE @TotalClientes INT = (SELECT COUNT(*) FROM Clientes);

INSERT INTO Pedidos (ClienteId, Status, ValorTotal, DataPedido)
SELECT
    ((ABS(CHECKSUM(NEWID())) % @TotalClientes) + 1) AS ClienteId,
    ((ABS(CHECKSUM(NEWID())) % 5) + 1) AS Status,
    CAST((ABS(CHECKSUM(NEWID())) % 500000) / 100.0 AS DECIMAL(18, 2)) AS ValorTotal,
    DATEADD(SECOND, -ABS(CHECKSUM(NEWID())) % 63072000, SYSUTCDATETIME()) AS DataPedido
FROM #Numbers
WHERE n <= 500;
GO

INSERT INTO ItensPedido (PedidoId, NomeProduto, Quantidade, PrecoUnitario)
SELECT
    p.Id,
    CONCAT('Produto ', ((ABS(CHECKSUM(NEWID())) % 200) + 1)) AS NomeProduto,
    ((ABS(CHECKSUM(NEWID())) % 5) + 1) AS Quantidade,
    CAST((ABS(CHECKSUM(NEWID())) % 20000) / 100.0 + 1 AS DECIMAL(18, 2)) AS PrecoUnitario
FROM Pedidos p
CROSS APPLY (SELECT ((ABS(CHECKSUM(NEWID())) % 3) + 1) AS QtdItens) qtd
CROSS APPLY (SELECT n FROM #Numbers WHERE n <= qtd.QtdItens) itens;
GO

DROP TABLE #Numbers;
GO

PRINT 'Seed CI concluído.';
SELECT
    (SELECT COUNT(*) FROM Clientes) AS TotalClientes,
    (SELECT COUNT(*) FROM Pedidos) AS TotalPedidos,
    (SELECT COUNT(*) FROM ItensPedido) AS TotalItensPedido;
GO
