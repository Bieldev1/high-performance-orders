# Benchmarks

Dados reais, extraídos rodando os scripts `database/scripts/03-bad-queries.sql` e
`04-optimized-queries.sql` no SQL Server (via `sqlcmd`, com `SET STATISTICS IO/TIME ON`),
contra a base já populada por `02-seed-data.sql` (5.000 clientes, 500.000 pedidos,
~1.000.000 itens). Se você tiver acesso ao SSMS/Azure Data Studio, rode os dois scripts
com **"Include Actual Execution Plan"** ligado — é a forma mais direta de ver o Table
Scan e os Key Lookups da consulta ruim virarem Index Seeks na otimizada.

## Consulta "ruim" (`03-bad-queries.sql`)

Lista pedidos com cliente e itens via `JOIN`, ordenando por `DataPedido` (não é coluna
líder de nenhum índice) e paginando com `OFFSET 100000 ROWS FETCH NEXT 20 ROWS ONLY`.

```
Table 'ItensPedido'. Scan count 13, logical reads 6646
Table 'Pedidos'.     Scan count 13, logical reads 2826
Table 'Clientes'.    Scan count 13, logical reads 121
-----------------------------------------------------
Total logical reads: 9.593

CPU time = 8213 ms, elapsed time = 1250 ms
```

**Por que é ruim:** sem filtro por `Status`/`ValorTotal`, o SQL Server não consegue usar
os índices compostos — cai em Table Scan em `Pedidos`, e o `OFFSET` obriga a varrer e
descartar 100.020 linhas antes de devolver as 20 desejadas. O `Scan count 13` indica que
o otimizador paralelizou a varredura em 13 threads (custo alto o suficiente pra isso valer
a pena), sinal claro de consulta cara.

## Consulta otimizada (`04-optimized-queries.sql`)

Filtra por `Status`, pagina por keyset (`WHERE Id > @UltimoId`), e usa subquery para
contar itens em vez de `JOIN`.

```
Table 'Pedidos'.     Scan count 1, logical reads 3
Table 'ItensPedido'. Scan count 1, logical reads 7
-----------------------------------------------------
Total logical reads: 10

CPU time = 4 ms, elapsed time = 0-18 ms
```

**Por que é rápida:** `Status = 2` casa com a coluna líder de `IX_Pedidos_Covering`, o
SQL Server faz Index Seek direto nela; como o índice já inclui (`INCLUDE`) todas as
colunas do `SELECT`, não precisa voltar na tabela base (sem Key Lookup). A contagem de
itens usa `IX_ItensPedido_PedidoId`, também covering — index-only, sem tocar a tabela.

## Comparativo

| Métrica | Consulta ruim | Consulta otimizada | Ganho |
|---|---|---|---|
| Logical reads | 9.593 | 10 | ~960x menos I/O |
| Elapsed time | ~1.250 ms | ~0-18 ms | ~70-1000x mais rápido |
| Scan count (Pedidos) | 13 (paralelo) | 1 | Sem paralelismo necessário |
| Padrão de acesso | Table Scan | Index Seek | — |

## Endpoints da API (`/api/pedidos/*`)

Rodando contra a mesma base, via HTTP:

| Endpoint | Tempo (1ª chamada / fria) | Tempo (aquecida) |
|---|---|---|
| `GET /slow?page=1&pageSize=20` | ~3.700-4.200 ms | ~800-1.500 ms* |
| `GET /fast?pageSize=20` | ~2.500 ms (compilação da query LINQ) | ~120-460 ms |
| `GET /fast-dapper?pageSize=20` | ~450 ms | ~390 ms |

\* mesmo aquecido, o `/slow` continua caro porque o custo real está nas N+1 queries
(uma de Cliente e uma de Itens por Pedido retornado), não na compilação.

**EF Core vs Dapper:** aquecido (query já compilada/plano em cache), a diferença entre
`/fast` (EF Core) e `/fast-dapper` é pequena — o EF Core 8 já é bem otimizado quando não
precisa recompilar a query. A vantagem do Dapper aparece mais: (1) na primeira chamada,
por não ter custo de compilação de expression tree; (2) sob alta concorrência, por não
pagar overhead de tracking/materialização de entidades (aqui já mitigado com
`AsNoTracking`); (3) em código mais simples/direto para quem já conhece SQL.

## Método usado

1. Rodar a query em um cliente SQL (`sqlcmd`, SSMS ou Azure Data Studio) com
   `SET STATISTICS IO ON` e `SET STATISTICS TIME ON`.
2. Ler o "logical reads" por tabela na aba de mensagens — é o número de páginas de 8KB
   lidas do buffer cache, a métrica mais estável pra comparar consultas (não varia com
   carga da máquina como o tempo de execução varia).
3. Comparar com **Actual Execution Plan** (SSMS/ADS) para confirmar visualmente:
   Table Scan/Key Lookup na versão ruim, Index Seek na otimizada.
4. Só depois, medir tempo de parede via `/api/pedidos/benchmark` (Stopwatch real,
   logical reads via `sys.dm_exec_query_stats`) para ter um número único, reprodutível
   via HTTP, útil pra comparação rápida sem abrir um cliente SQL.
