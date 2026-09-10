# Planos de execução

Planos **reais** (actual execution plans, capturados com `SET STATISTICS XML ON`, não estimados)
das duas consultas de `database/scripts/`, contra a base de 500k pedidos.

## Como abrir

- **SSMS / Azure Data Studio**: `File → Open → File`, selecione o `.sqlplan` — abre o plano
  gráfico, com os operadores, custo relativo (%) de cada um e o número de linhas real vs estimado.
- **VS Code**: extensão *SQL Server (mssql)* também renderiza `.sqlplan`.
- São só XML — dá pra abrir num editor de texto e ler os atributos `PhysicalOp`, mas o valor
  está na visualização gráfica.

## `03-bad-query.sqlplan`

Consulta de `database/scripts/03-bad-queries.sql` (JOIN de Pedido + Cliente + Itens, `ORDER BY
DataPedido`, `OFFSET 100000`).

Operadores que aparecem:

| Operador | Quantas vezes | O que significa aqui |
|---|---|---|
| **Clustered Index Scan** | 3 | Varredura completa de `PK_Pedidos`, `PK_Clientes` e `PK_ItensPedido` — nenhum índice útil para o filtro/ordenação, então lê as tabelas inteiras |
| **Hash Match** | 2 | Os dois JOINs feitos por hash (custoso: materializa uma tabela de hash em memória/tempdb) |
| **Sort** | 1 | `ORDER BY DataPedido` precisa de uma ordenação explícita — nenhum índice entrega essa ordem |
| **Parallelism** | 1 | A consulta é cara o suficiente para o otimizador dividir em várias threads |
| **Top** | 1 | O `FETCH NEXT 20` — mas só depois de já ter processado as 100.020 linhas do offset |

## `04-optimized-query.sqlplan`

Consulta de `database/scripts/04-optimized-queries.sql` (filtro por `Status`, keyset pagination
por `Id`, `TOP 20`, subquery de `COUNT` para os itens).

| Operador | Quantas vezes | O que significa aqui |
|---|---|---|
| **Clustered Index Seek** (`PK_Pedidos`) | 1 | Seek pelo range `Id > @UltimoId`, já na ordem do `ORDER BY Id` — sem Sort |
| **Index Seek** (`IX_ItensPedido_PedidoId`) | 1 | A contagem de itens usa o índice de `ItensPedido`, sem tocar a tabela base |
| **Merge Join** | 1 | Join por merge (mais barato que Hash Match) — possível porque as duas entradas já vêm ordenadas pela chave |
| **Stream Aggregate** | 1 | O `COUNT(*)` da subquery, em streaming — custo mínimo |
| — | — | **Não há Sort nem Parallelism** |

### Nota honesta sobre o covering index

Neste plano específico, o SQL Server escolheu fazer o **Seek pela chave primária** (`Id >
@UltimoId`) e aplicar `Status = 2` como *residual predicate*, em vez de fazer o seek por
`IX_Pedidos_Covering` (que tem `Status` como coluna líder). Isso acontece porque, com `TOP 20`
+ `ORDER BY Id` + `@UltimoId = 0`, varrer o começo da PK e descartar os poucos que não têm
`Status = 2` é mais barato do que usar o covering index e depois reordenar por `Id`.

Com um `@UltimoId` mais alto, ou sem o `TOP` pequeno, ou ordenando por `ValorTotal`, o
otimizador passa a preferir o `IX_Pedidos_Covering`. Ver `docs/index-strategy.md` — o índice
não é "inútil", ele cobre outros padrões de consulta; este caso só não é o cenário em que ele
ganha. É um bom lembrete de que o plano depende dos parâmetros e do volume, não só do índice
existir.

## Regenerar

```bash
# com o container hpo-sqlserver rodando e a base populada:
docker cp database/scripts/03-bad-queries.sql hpo-sqlserver:/tmp/
docker exec hpo-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'Your_password123' -C -d HighPerformanceOrders -y 0 \
  -Q "SET STATISTICS XML ON; <cole a query aqui>" \
  | awk '/<ShowPlanXML/,/<\/ShowPlanXML>/' > docs/execution-plans/03-bad-query.sqlplan
```
