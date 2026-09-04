# Indexing (índices)

Um índice não-clusterizado no SQL Server é uma estrutura de dados separada (uma B-tree)
que guarda uma cópia ordenada de algumas colunas de uma tabela, com um ponteiro de volta
pra linha original. Sem índice, qualquer filtro (`WHERE`) obriga o SQL Server a ler a
tabela inteira (**Table Scan**) pra achar as linhas que interessam.

## Chave do índice vs INCLUDE

- **Colunas na chave** ficam ordenadas dentro do índice — servem pra `WHERE` (igualdade
  e range) e `ORDER BY`. Custam mais (o índice inteiro é reordenado a cada
  insert/update/delete que as toque).
- **Colunas em `INCLUDE`** só ficam "penduradas" na folha do índice, sem ordenação —
  servem só pra evitar voltar na tabela base quando o `SELECT` precisa delas. Mais
  baratas de manter.

## Ordem das colunas na chave

Regra prática: **igualdade antes de range**. Se a consulta faz
`WHERE Status = 2 AND ValorTotal > 100`, `Status` deve vir primeiro na chave — assim o
SQL Server usa o índice pra pular direto pros registros com `Status = 2` (Seek), e dentro
desse subconjunto já ordenado por `ValorTotal` aplica o filtro de range. Trocar a ordem
(range antes de igualdade) quebra essa eficiência.

## Trade-off

Todo índice acelera leitura e atrapalha escrita (mais uma estrutura pra atualizar a cada
INSERT/UPDATE/DELETE). Ver [index-strategy.md](../index-strategy.md) para os índices
específicos deste projeto e por que cada um existe.
