# Execution Plan

É o "roteiro" que o otimizador do SQL Server escolhe pra executar uma consulta: quais
índices usar, em que ordem fazer os `JOIN`s, se precisa ordenar em memória, etc. Duas
formas de ver:

- **Estimated Execution Plan** (Ctrl+L no SSMS): mostra o plano sem rodar a query,
  baseado em estatísticas.
- **Actual Execution Plan** (Ctrl+M, depois roda a query): mostra o plano realmente
  usado, com números reais de linhas processadas — mais confiável, porque estatísticas
  desatualizadas podem enganar o estimado.

## Operadores que mais importam pra performance

| Operador | O que significa | Bom ou ruim? |
|---|---|---|
| **Table Scan** | Lê a tabela inteira, linha por linha | Ruim em tabela grande sem filtro seletivo |
| **Index Scan** | Lê o índice inteiro (não a tabela) | Melhor que Table Scan, mas ainda lê tudo |
| **Index Seek** | Pula direto pras linhas que interessam, usando a árvore do índice | Bom — é o que se busca |
| **Key Lookup** | Depois de um Seek/Scan no índice, volta na tabela buscar colunas que faltam | Ruim em volume — ver [covering-index.md](covering-index.md) |
| **Sort** | Ordena resultado em memória/tempdb | Custa CPU/memória; evitável se um índice já entrega a ordem certa |

## SET STATISTICS IO / TIME

Complementam o plano com números concretos:

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
```

- **`logical reads`**: quantas páginas de 8KB foram lidas do buffer cache. É a métrica
  mais estável pra comparar duas versões de uma consulta — não varia com carga da
  máquina como o "elapsed time" varia.
- **`CPU time` / `elapsed time`**: tempo real gasto. Útil, mas pode variar entre
  execuções por fatores externos (cache frio, outras queries concorrentes).

Este projeto usa exatamente essa combinação — ver [benchmarks.md](../benchmarks.md) para
os números reais coletados comparando a consulta lenta e a otimizada.
