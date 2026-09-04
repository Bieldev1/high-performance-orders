using Api.Application.Models.Pedidos;
using Domain.AggregatesModel.PedidoAggregate;

namespace Api.Application.Queries.Pedidos;

public interface IPedidoDapperQueries
{
    /// <summary>
    /// Mesma consulta otimizada do GetFastAsync (keyset pagination + covering index), mas via Dapper/SQL puro,
    /// para comparar o overhead de tracking/materialização do EF Core frente a um mapeamento direto.
    /// </summary>
    Task<List<PedidoFastModel>> GetFastAsync(long? lastId, int pageSize, StatusPedido? status);
}
