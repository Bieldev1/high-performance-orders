-- Popula o banco com dados suficientes para demonstrar diferenças reais de performance:
-- ~5.000 clientes, ~500.000 pedidos e ~1 a 3 itens por pedido (~1.000.000 registros em ItensPedido).
-- Geração 100% set-based (sem loops/cursors) para não levar horas.

USE HighPerformanceOrders;
GO

SET NOCOUNT ON;

TRUNCATE TABLE ItensPedido;
DELETE FROM Pedidos;
DELETE FROM Clientes;
GO

-- Tabela auxiliar de números, usada para gerar N linhas via CROSS JOIN.
IF OBJECT_ID('tempdb..#Numbers') IS NOT NULL DROP TABLE #Numbers;

WITH L0 AS (SELECT 1 AS c UNION ALL SELECT 1),
     L1 AS (SELECT 1 AS c FROM L0 A CROSS JOIN L0 B),
     L2 AS (SELECT 1 AS c FROM L1 A CROSS JOIN L1 B),
     L3 AS (SELECT 1 AS c FROM L2 A CROSS JOIN L2 B),
     L4 AS (SELECT 1 AS c FROM L3 A CROSS JOIN L3 B),
     L5 AS (SELECT 1 AS c FROM L4 A CROSS JOIN L3 B),
     Nums AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM L5)
SELECT TOP (600000) n
INTO #Numbers
FROM Nums
ORDER BY n;

CREATE UNIQUE CLUSTERED INDEX IX_Numbers_n ON #Numbers (n);
GO

-- ==========================================================================
-- Clientes (~5.000)
-- ==========================================================================
INSERT INTO Clientes (Nome, Email, DataCadastro)
SELECT
    CONCAT('Cliente ', n),
    CONCAT('cliente', n, '@teste.com'),
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 1460, SYSUTCDATETIME())
FROM #Numbers
WHERE n <= 5000;
GO

-- ==========================================================================
-- Pedidos (~500.000), distribuídos entre os clientes e com Status/ValorTotal/DataPedido variados
-- ==========================================================================
DECLARE @TotalClientes INT = (SELECT COUNT(*) FROM Clientes);

INSERT INTO Pedidos (ClienteId, Status, ValorTotal, DataPedido)
SELECT
    ((ABS(CHECKSUM(NEWID())) % @TotalClientes) + 1) AS ClienteId,
    ((ABS(CHECKSUM(NEWID())) % 5) + 1) AS Status,
    CAST((ABS(CHECKSUM(NEWID())) % 500000) / 100.0 AS DECIMAL(18, 2)) AS ValorTotal,
    DATEADD(SECOND, -ABS(CHECKSUM(NEWID())) % 63072000, SYSUTCDATETIME()) AS DataPedido
FROM #Numbers
WHERE n <= 500000;
GO

-- ==========================================================================
-- ItensPedido (1 a 3 itens por pedido, ~1.000.000 de linhas)
-- CROSS APPLY garante que a quantidade de itens é decidida uma vez por pedido.
-- ==========================================================================
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

PRINT 'Seed concluído.';
SELECT
    (SELECT COUNT(*) FROM Clientes) AS TotalClientes,
    (SELECT COUNT(*) FROM Pedidos) AS TotalPedidos,
    (SELECT COUNT(*) FROM ItensPedido) AS TotalItensPedido;
GO
