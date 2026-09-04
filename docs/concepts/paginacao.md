# Paginação: OFFSET vs Keyset

## OFFSET (usada no `/orders/slow`)

```sql
ORDER BY DataPedido
OFFSET @pagina * @tamanhoPagina ROWS FETCH NEXT @tamanhoPagina ROWS ONLY;
```

Simples de implementar e permite pular direto pra "página 50", mas o SQL Server
**precisa processar todas as linhas anteriores ao offset** antes de descartá-las. Página
1 é barata; página 5.000 (offset de 100.000 linhas) é cara — o custo cresce linearmente
com o número da página, mesmo devolvendo sempre o mesmo tanto de linhas. É exatamente o
que os números de `03-bad-queries.sql` mostram (ver [benchmarks.md](../benchmarks.md)).

## Keyset / Cursor-based (usada no `/orders/fast`)

```sql
WHERE Id > @ultimoIdDaPaginaAnterior
ORDER BY Id
FETCH NEXT @tamanhoPagina ROWS ONLY;
```

Em vez de "pule N linhas", a pergunta é "me dê as próximas linhas depois deste ponto".
Isso vira um **Index Seek** direto na posição certa da B-tree — custo praticamente
constante, independente de qual "página" (ponto do cursor) você está. O preço é perder a
capacidade de pular direto pra uma página arbitrária (não dá pra pedir "página 500" sem
ter navegado até lá) e precisar expor um cursor (aqui, o `Id` do último item da página
anterior) em vez de um número de página simples.

## Quando usar cada uma

| | OFFSET | Keyset |
|---|---|---|
| UI com números de página (1, 2, 3...) | ✅ Necessário | ❌ Não dá pra pular direto |
| Rolagem infinita / "carregar mais" | Funciona, mas degrada | ✅ Ideal |
| Tabelas grandes (milhões de linhas) | Degrada rápido em páginas altas | ✅ Custo constante |
| Coluna de cursor precisa ser única e ordenável | Não importa | ✅ Obrigatório (aqui, `Id`) |

Este projeto usa keyset por `Id` (chave primária, sempre única e sequencial) — a forma
mais simples e robusta de cursor. Um cursor composto (ex: `ValorTotal` + `Id` como
desempate) seria necessário se a ordenação fosse por uma coluna não-única.
