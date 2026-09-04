# Problema N+1

Acontece quando o código faz **1 consulta** pra buscar uma lista de N registros, e depois
**mais N consultas** (uma por registro) pra buscar dados relacionados — em vez de trazer
tudo em menos consultas (idealmente 1 ou 2).

## Exemplo deste projeto (`GetSlowAsync`)

```csharp
var pedidos = await context.Pedidos.Skip(...).Take(pageSize).ToListAsync(); // 1 query

foreach (var pedido in pedidos)
{
    var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Id == pedido.ClienteId); // N queries
    var itens = await context.ItensPedido.Where(i => i.PedidoId == pedido.Id).ToListAsync();  // N queries
}
```

Pra `pageSize = 20`, isso é **1 + 20 + 20 = 41 round trips** ao banco, cada um com a
latência de rede/parsing de uma conexão separada — mesmo que cada consulta individual
seja rápida, a soma das latências de rede domina o tempo total. É por isso que o
`/orders/slow` fica lento mesmo com poucos registros por página.

## Como evitar

- **EF Core**: `Include()` (faz um `JOIN` e materializa tudo numa consulta) — mas cuidado
  com Include excessivo em coleções grandes, que multiplica linhas (ver
  [system-design.md](../system-design.md)); ou uma projeção via `Select` que já traz o
  necessário (como faz o `/orders/fast`, com uma subquery de `COUNT` em vez de trazer os
  itens inteiros).
- **SQL puro/Dapper**: um `JOIN` bem filtrado, ou duas consultas (uma pros pedidos, outra
  pra todos os itens desses pedidos com `WHERE PedidoId IN (...)`) — 2 round trips em vez
  de N+1.

## Por que N+1 é enganoso em ambiente de dev

Em localhost, a latência de rede entre app e banco é quase zero, então N+1 parece
inofensivo. Em produção, com banco numa rede diferente (ou até outra região), cada round
trip extra custa alguns milissegundos — e 41 desses somam rápido.
