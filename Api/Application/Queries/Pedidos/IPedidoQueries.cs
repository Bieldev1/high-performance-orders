using Api.Application.Models.Pedidos;
using Domain.AggregatesModel.PedidoAggregate;

namespace Api.Application.Queries.Pedidos;

public interface IPedidoQueries
{
    /// <summary>
    /// Versão propositalmente ruim: OFFSET pagination, N+1 (uma query de Cliente e outra de Itens por Pedido) e tracking desnecessário.
    /// </summary>
    Task<List<PedidoSlowModel>> GetSlowAsync(int page, int pageSize);

    /// <summary>
    /// Versão otimizada: keyset pagination, projeção via Select, AsNoTracking e filtro por Status usando o covering index.
    /// </summary>
    Task<List<PedidoFastModel>> GetFastAsync(long? lastId, int pageSize, StatusPedido? status);

    /// <summary>
    /// Executa as duas versões (lenta e otimizada) medindo tempo de execução e logical reads via SET STATISTICS IO.
    /// </summary>
    Task<BenchmarkResultModel> BenchmarkAsync(int pageSize);
}
