# Estratégia de índices

Os três índices deste projeto (`database/scripts/01-create-tables.sql`) não foram
definidos antes de rodar nada — foram criados **depois** de rodar a consulta lenta
(`03-bad-queries.sql`) com `SET STATISTICS IO ON` e ver o Table Scan em `Pedidos` e os
Key Lookups no plano de execução. Essa ordem importa: índice sem consulta real pra
justificar é só chute.

## IX_Pedidos_Covering

```sql
CREATE INDEX IX_Pedidos_Covering
ON Pedidos (Status, ValorTotal)
INCLUDE (DataPedido, ClienteId, Id);
```

- **Ordem das colunas chave:** `Status` primeiro porque é o filtro de igualdade mais
  usado na listagem (`WHERE Status = @status`) e tem baixa cardinalidade (5 valores) —
  colunas de igualdade vêm antes de colunas de range/order na chave de um índice
  não-clusterizado. `ValorTotal` em seguida cobre filtros de faixa (`ValorTotal BETWEEN`)
  e ordenação secundária.
- **Por que `INCLUDE` e não chave:** `DataPedido`, `ClienteId` e `Id` aparecem no
  `SELECT` mas não em `WHERE`/`ORDER BY` frequente o suficiente pra justificar entrar na
  chave (que é ordenada e mais cara de manter). Colocá-las em `INCLUDE` faz o índice virar
  **covering**: o SQL Server responde a query só com as páginas de folha do índice, sem
  Key Lookup na tabela base.
- **Trade-off:** todo `INSERT`/`UPDATE` em `Pedidos` que toque `Status`, `ValorTotal` ou
  qualquer coluna do `INCLUDE` paga o custo de manter esse índice atualizado. Pra uma
  tabela de pedidos (muito mais lida que atualizada após criada), vale a pena.

## IX_Pedidos_Status_ValorTotal_DataPedido

```sql
CREATE INDEX IX_Pedidos_Status_ValorTotal_DataPedido
ON Pedidos (Status, ValorTotal, DataPedido DESC)
INCLUDE (ClienteId);
```

- **Por que existe além do covering acima:** esse aqui serve consultas que **ordenam**
  por `DataPedido` dentro de um `Status`/`ValorTotal` já filtrado (ex: "pedidos pagos,
  mais recentes primeiro") — o `DESC` na chave permite ao SQL Server ler o índice na
  ordem física desejada, sem um `Sort` explícito no plano.
- **Trade-off:** dois índices parecidos (`Covering` e este) sobre as mesmas duas colunas
  líderes custam espaço em disco e overhead de manutenção duplicado. Em um cenário real,
  eu mediria se as consultas que precisam do `ORDER BY DataPedido` são frequentes o
  suficiente pra justificar manter os dois, ou se um único índice bem desenhado (esse,
  com `DataPedido` na chave) serviria as duas consultas — ficou separado aqui de
  propósito, para efeito didático de comparar covering simples vs covering com order by.

## IX_ItensPedido_PedidoId

```sql
CREATE INDEX IX_ItensPedido_PedidoId
ON ItensPedido (PedidoId)
INCLUDE (NomeProduto, Quantidade)
WITH (DROP_EXISTING = ON);
```

- **Por que existe:** todo `FOREIGN KEY` no SQL Server *não* ganha índice automático na
  tabela filha (diferente do que muita gente assume) — sem esse índice, contar/buscar
  itens de um pedido específico (`WHERE PedidoId = @id`) seria Table Scan em
  `ItensPedido`. O EF Core, ao gerar a migration, cria um índice simples em `PedidoId`
  (suporte padrão de FK); este script o **upgrada** para covering via
  `WITH (DROP_EXISTING = ON)`, evitando duplicar o índice.
- **Por que `INCLUDE (NomeProduto, Quantidade)`:** são exatamente as colunas que o
  endpoint `/orders/slow` (e a listagem de itens de um pedido) precisa — outra vez,
  index-only em vez de Key Lookup.
- **Trade-off:** se um dia a tabela `ItensPedido` precisar de buscas por `NomeProduto`
  (ex: "todos os itens do Produto X"), esse índice não ajuda — seria necessário outro,
  com `NomeProduto` na chave.

## Por que os índices não estão na Fluent API do EF Core

Ver [system-design.md](system-design.md). Resumindo: tuning de índice é um processo
iterativo baseado em consultas reais e planos de execução reais, não uma decisão que se
toma no momento de desenhar a entidade. Declarar via Fluent API antecipa uma decisão que
só faz sentido depois de medir — por isso os índices vivem em
`database/scripts/01-create-tables.sql`, versionados como SQL puro, aplicados depois da
migration criar o schema base.

## Como validar

1. Rodar `03-bad-queries.sql` e `04-optimized-queries.sql` (ver [benchmarks.md](benchmarks.md)
   para os números reais coletados).
2. `sys.dm_db_index_usage_stats` para confirmar que o índice está sendo de fato usado em
   produção/teste, não só existindo:
   ```sql
   SELECT i.name, s.user_seeks, s.user_scans, s.user_lookups
   FROM sys.dm_db_index_usage_stats s
   JOIN sys.indexes i ON i.object_id = s.object_id AND i.index_id = s.index_id
   WHERE s.object_id = OBJECT_ID('Pedidos');
   ```
3. `sys.dm_db_missing_index_details` de tempos em tempos, pra ver se o otimizador está
   pedindo um índice que ainda não existe — mas sempre validando manualmente antes de
   criar (a sugestão do SQL Server é um ponto de partida, não uma ordem).
