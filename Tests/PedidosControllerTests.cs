using System.Net;
using System.Net.Http.Json;
using Api.Application.Models.Pedidos;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests;

/// <summary>
/// Testes de integração reais: sobem a Api via WebApplicationFactory e batem no SQL Server
/// de verdade (mesma connection string do appsettings.json — o container hpo-sqlserver
/// precisa estar rodando e populado com database/scripts/01 e 02 antes de rodar estes testes).
/// </summary>
public class PedidosControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public PedidosControllerTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSlow_RetornaOk_ERespeitaPageSize()
    {
        var resposta = await client.GetAsync("/api/pedidos/slow?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var pedidos = await resposta.Content.ReadFromJsonAsync<List<PedidoSlowModel>>();
        Assert.NotNull(pedidos);
        Assert.True(pedidos!.Count <= 5);
    }

    [Fact]
    public async Task GetFast_RetornaOk_ERespeitaPageSize()
    {
        var resposta = await client.GetAsync("/api/pedidos/fast?pageSize=5");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var pedidos = await resposta.Content.ReadFromJsonAsync<List<PedidoFastModel>>();
        Assert.NotNull(pedidos);
        Assert.True(pedidos!.Count <= 5);
    }

    [Fact]
    public async Task GetFastDapper_RetornaOk_EMesmoFormatoDoFast()
    {
        var resposta = await client.GetAsync("/api/pedidos/fast-dapper?pageSize=5");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var pedidos = await resposta.Content.ReadFromJsonAsync<List<PedidoFastModel>>();
        Assert.NotNull(pedidos);
        Assert.True(pedidos!.Count <= 5);
    }

    [Fact]
    public async Task GetFast_KeysetPagination_NaoRepeteNemPulaRegistrosEntrePaginas()
    {
        var primeiraPagina = await client.GetFromJsonAsync<List<PedidoFastModel>>("/api/pedidos/fast?pageSize=5");
        Assert.NotNull(primeiraPagina);
        Assert.NotEmpty(primeiraPagina!);

        var ultimoIdPrimeiraPagina = primeiraPagina!.Last().Id;

        var segundaPagina = await client.GetFromJsonAsync<List<PedidoFastModel>>(
            $"/api/pedidos/fast?pageSize=5&lastId={ultimoIdPrimeiraPagina}");
        Assert.NotNull(segundaPagina);

        var idsPrimeiraPagina = primeiraPagina.Select(p => p.Id).ToHashSet();
        Assert.All(segundaPagina!, pedido => Assert.DoesNotContain(pedido.Id, idsPrimeiraPagina));
        Assert.All(segundaPagina!, pedido => Assert.True(pedido.Id > ultimoIdPrimeiraPagina));
    }

    [Fact]
    public async Task GetBenchmark_RetornaOk_ComTemposDeExecucaoNaoNegativos()
    {
        var resultado = await client.GetFromJsonAsync<BenchmarkResultModel>("/api/pedidos/benchmark?pageSize=10");

        Assert.NotNull(resultado);
        Assert.True(resultado!.SlowQuery.ExecutionTimeMs >= 0);
        Assert.True(resultado.OptimizedQuery.ExecutionTimeMs >= 0);
    }
}
