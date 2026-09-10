# High Performance Orders API

Projeto de estudo e portfólio focado em performance de queries SQL Server, análise de
execution plan e otimização com EF Core, usando uma API .NET 8 que simula um sistema de
pedidos (e-commerce). Todo o domínio é em português (`Pedido`, `Cliente`, `ItemPedido`),
seguindo o mesmo padrão arquitetural (DDD + CQRS leve) usado em projetos reais.

## Stack

- .NET 8 (ASP.NET Core Web API)
- EF Core 8 (SQL Server) + Dapper (comparação de performance)
- SQL Server 2022 (Docker)
- Swagger/Swashbuckle

## Arquitetura

```
HighPerformanceOrders.sln
├── Api/
│   ├── Controllers/
│   ├── Application/
│   │   ├── Queries/          (PedidoQueries, PedidoDapperQueries)
│   │   └── Models/           (Models de request/response, sufixo *Model)
│   └── Configurations/       (DI, Swagger, GlobalExceptionHandler)
├── Domain/                   (sem dependências externas)
│   ├── AggregatesModel/      (PedidoAggregate, ClienteAggregate)
│   └── SeedWork/             (Entity, IAggregateRoot)
├── Infrastructure/
│   ├── Data/                 (AppDbContext, EntityConfigurations)
│   └── Migrations/
└── Tests/                    (testes de integração — xUnit + WebApplicationFactory)

database/scripts/             (schema + índices + seed + queries de estudo, em SQL puro)
docs/                         (system design, benchmarks, estratégia de índices, conceitos, planos de execução)
.github/workflows/ci.yml      (CI: build + testes + docker; CD: publish da imagem no GHCR)
```

> Este projeto é só leitura (endpoints de estudo de performance) — por isso não há
> `Repository`/`UnitOfWork`/`Command`/`Result Pattern`: essas camadas existiram numa
> versão anterior mas foram removidas por não terem uso real (nenhum endpoint de escrita
> foi implementado). Ver [docs/system-design.md](docs/system-design.md).

Detalhes da arquitetura e o raciocínio por trás das decisões: [docs/system-design.md](docs/system-design.md).

## Como rodar

### Com Docker (recomendado)

```bash
docker compose up -d --build
```

Sobe `hpo-sqlserver` (SQL Server, porta 1433) e `hpo-api` (porta 8080). A Api espera o
banco ficar saudável (`healthcheck`) antes de iniciar.

Depois, aplique o schema, os índices e o seed de dados (~500k pedidos):

```bash
docker cp database/scripts/01-create-tables.sql hpo-sqlserver:/tmp/01-create-tables.sql
docker cp database/scripts/02-seed-data.sql hpo-sqlserver:/tmp/02-seed-data.sql

docker exec hpo-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_password123" -C -i /tmp/01-create-tables.sql
docker exec hpo-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_password123" -C -i /tmp/02-seed-data.sql
```

Swagger: http://localhost:8080/swagger

### Localmente (sem Docker na Api)

```bash
docker compose up -d sqlserver
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj
dotnet run --project Api
```

## Endpoints

Todos em `/api/pedidos`:

| Endpoint | O que demonstra |
|---|---|
| `GET /slow?page=1&pageSize=20` | Anti-padrões: OFFSET pagination, N+1 (Cliente e Itens buscados um por um), sem `AsNoTracking` |
| `GET /fast?lastId=0&pageSize=20&status=2` | Keyset pagination, projeção via `Select`, `AsNoTracking`, filtro casando com covering index |
| `GET /fast-dapper?...` | Mesma consulta otimizada, via Dapper/SQL puro — comparação EF Core vs Dapper |
| `GET /benchmark?pageSize=20` | Roda slow vs fast e mede tempo real + logical reads |

Erros não tratados retornam `ProblemDetails` (RFC 7807) com um `traceId` que aparece
também no log da API, para correlação.

## Banco de dados

`database/scripts/`:

1. `01-create-tables.sql` — schema (exportado da migration do EF) + índices de performance (SQL puro, criados manualmente depois de analisar o plano de execução real — ver [docs/index-strategy.md](docs/index-strategy.md))
2. `02-seed-data.sql` — gera ~5.000 clientes, ~500.000 pedidos e ~1.000.000 itens, 100% set-based
3. `03-bad-queries.sql` — a consulta lenta, pra rodar manualmente com `SET STATISTICS IO/TIME ON` e ver o Table Scan
4. `04-optimized-queries.sql` — a versão otimizada, pra comparar
5. `02-seed-data-ci.sql` — seed reduzido (~500 pedidos) usado só pelo pipeline de CI

Os planos de execução reais dessas queries estão em [`docs/execution-plans/`](docs/execution-plans/) (`.sqlplan`, abrem no SSMS).

## Documentação

- [docs/system-design.md](docs/system-design.md) — arquitetura em camadas e princípios seguidos
- [docs/benchmarks.md](docs/benchmarks.md) — números reais coletados (logical reads, tempo de execução, EF Core vs Dapper)
- [docs/index-strategy.md](docs/index-strategy.md) — por que cada índice existe, ordem das colunas, trade-offs
- [docs/concepts/](docs/concepts) — indexing, covering index, execution plan, N+1, paginação (OFFSET vs keyset)
- [docs/execution-plans/](docs/execution-plans) — planos de execução reais (`.sqlplan`) + leitura operador a operador

## CI / CD

`.github/workflows/ci.yml`, disparado em push e pull request para `main`/`develop`:

| Job | O que faz |
|---|---|
| `build` | `dotnet restore` + `build` em Release (com cache de pacotes NuGet) |
| `test` | Sobe um SQL Server efêmero (service container), aplica schema + seed reduzido, roda os testes de integração reais |
| `docker` | Valida que `Api/Dockerfile` builda |
| `publish` | **Só em push para `main`**: builda e publica a imagem em `ghcr.io/bieldev1/high-performance-orders-api` (tags `:latest` e `:<sha>`) |

`main` tem branch protection: exige os 3 checks (`build`/`test`/`docker`) verdes, sem force-push, aplicável a admins — merge só via Pull Request.

```bash
docker pull ghcr.io/bieldev1/high-performance-orders-api:latest
```

## Git Flow

- `main` → produção (estável, protegida — merge só via PR com CI verde)
- `develop` → integração
- `feature/*` → novas funcionalidades, criadas a partir de `develop`

Commits semânticos: `feat:`, `perf:`, `fix:`, `docs:`, `refactor:`, `ci:`, `test:`, `chore:`.

## Licença

[MIT](LICENSE).
