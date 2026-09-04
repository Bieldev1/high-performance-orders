-- Mesma necessidade (listar pedidos paginados), mas filtrando por Status (casa com a coluna
-- líder de IX_Pedidos_Covering/IX_Pedidos_Status_ValorTotal_DataPedido) e paginando por keyset
-- (WHERE Id > @ultimoId) em vez de OFFSET. Sem JOIN com Clientes/ItensPedido: a contagem de itens
-- vem de uma subquery que usa o índice de ItensPedido (index-only, sem tocar a tabela base).

USE HighPerformanceOrders;
GO

SET STATISTICS IO ON;
SET STATISTICS TIME ON;
GO

DECLARE @UltimoId BIGINT = 0;

SELECT TOP (20)
    p.Id,
    p.ClienteId,
    p.Status,
    p.ValorTotal,
    p.DataPedido,
    (SELECT COUNT(*) FROM ItensPedido i WHERE i.PedidoId = p.Id) AS QuantidadeItens
FROM Pedidos p
WHERE p.Status = 2
  AND p.Id > @UltimoId
ORDER BY p.Id;
GO

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
GO
