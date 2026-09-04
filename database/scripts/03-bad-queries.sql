-- Consulta "ruim": junta Pedido + Cliente + Itens numa tanada só, ordena por uma coluna
-- que não é líder de nenhum índice (DataPedido) e pagina com OFFSET/FETCH.
-- Rode no SSMS/Azure Data Studio com "Include Actual Execution Plan" ligado para ver
-- o Table Scan em Pedidos e os Key Lookups causados pelo JOIN com ItensPedido.

USE HighPerformanceOrders;
GO

SET STATISTICS IO ON;
SET STATISTICS TIME ON;
GO

SELECT
    p.Id,
    c.Nome AS ClienteNome,
    p.Status,
    p.ValorTotal,
    p.DataPedido,
    i.NomeProduto,
    i.Quantidade,
    i.PrecoUnitario
FROM Pedidos p
JOIN Clientes c ON c.Id = p.ClienteId
LEFT JOIN ItensPedido i ON i.PedidoId = p.Id
ORDER BY p.DataPedido
OFFSET 100000 ROWS FETCH NEXT 20 ROWS ONLY;
GO

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
GO
