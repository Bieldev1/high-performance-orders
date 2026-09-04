using Api.Application.Models.Pedidos;
using Api.Application.Queries.Pedidos;
using Domain.AggregatesModel.PedidoAggregate;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Tags("Pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoQueries queries;
    private readonly IPedidoDapperQueries dapperQueries;

    public PedidosController(IPedidoQueries queries, IPedidoDapperQueries dapperQueries)
    {
        this.queries = queries;
        this.dapperQueries = dapperQueries;
    }

    /// <summary>
    /// Versão propositalmente ruim, para estudo: OFFSET pagination, N+1 e sem projeção.
    /// </summary>
    [HttpGet("slow")]
    [ProducesResponseType(typeof(List<PedidoSlowModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PedidoSlowModel>>> GetSlow([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await queries.GetSlowAsync(page, pageSize));

    /// <summary>
    /// Versão otimizada: keyset pagination, projeção via Select e índices adequados.
    /// </summary>
    [HttpGet("fast")]
    [ProducesResponseType(typeof(List<PedidoFastModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PedidoFastModel>>> GetFast(
        [FromQuery] long? lastId = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] StatusPedido? status = null)
        => Ok(await queries.GetFastAsync(lastId, pageSize, status));

    /// <summary>
    /// Mesma consulta otimizada do /fast, mas via Dapper/SQL puro — comparar com /fast para ver o
    /// overhead de tracking/materialização do EF Core mesmo numa query já otimizada.
    /// </summary>
    [HttpGet("fast-dapper")]
    [ProducesResponseType(typeof(List<PedidoFastModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PedidoFastModel>>> GetFastDapper(
        [FromQuery] long? lastId = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] StatusPedido? status = null)
        => Ok(await dapperQueries.GetFastAsync(lastId, pageSize, status));

    /// <summary>
    /// Compara slow vs fast, medindo tempo de execução e logical reads (SET STATISTICS IO) para a mesma carga.
    /// </summary>
    [HttpGet("benchmark")]
    [ProducesResponseType(typeof(BenchmarkResultModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<BenchmarkResultModel>> Benchmark([FromQuery] int pageSize = 20)
        => Ok(await queries.BenchmarkAsync(pageSize));
}
