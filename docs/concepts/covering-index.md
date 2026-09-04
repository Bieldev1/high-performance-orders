# Covering Index

Um índice é "covering" (cobridor) pra uma consulta específica quando ele sozinho contém
**todas** as colunas que a consulta precisa — as do `WHERE`/`ORDER BY` (na chave) e as do
`SELECT` (em `INCLUDE`). Nesse caso o SQL Server responde a consulta lendo só as páginas
de folha do índice, sem nunca tocar a tabela base.

## Sem covering: Key Lookup

Se o índice não tem todas as colunas necessárias, o SQL Server faz um **Key Lookup**
(ou RID Lookup, em tabela heap): pra cada linha encontrada no índice, volta na tabela
base pra buscar as colunas que faltam. Isso parece barato ("é só mais uma leitura"), mas
o custo é **por linha** — em 10.000 linhas encontradas, são 10.000 idas e vindas
aleatórias ao disco/buffer, o que muitas vezes é pior que um Table Scan sequencial.

## Exemplo deste projeto

```sql
CREATE INDEX IX_Pedidos_Covering
ON Pedidos (Status, ValorTotal)
INCLUDE (DataPedido, ClienteId, Id);
```

A query do endpoint `/orders/fast` (`SELECT Id, ClienteId, Status, ValorTotal, DataPedido
FROM Pedidos WHERE Status = @status`) usa só colunas que estão nesse índice — chave ou
`INCLUDE`. O plano de execução mostra **Index Seek** sem Key Lookup nenhum, contra o
Table Scan da versão sem filtro (ver [benchmarks.md](../benchmarks.md)).

## Trade-off

Covering index costuma ser mais largo (mais colunas em `INCLUDE`) que um índice comum, o
que aumenta o espaço em disco e o custo de manutenção. Vale a pena quando a consulta que
ele cobre é frequente/crítica o suficiente pra justificar.
