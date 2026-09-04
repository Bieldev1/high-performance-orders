using Api.Application.Models.Pedidos;
using Dapper;
using Domain.AggregatesModel.PedidoAggregate;
using Microsoft.Data.SqlClient;

namespace Api.Application.Queries.Pedidos;

public class PedidoDapperQueries : IPedidoDapperQueries
{
    private readonly string connectionString;

    public PedidoDapperQueries(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    public async Task<List<PedidoFastModel>> GetFastAsync(long? lastId, int pageSize, StatusPedido? status)
    {
        // Mesma estratégia do EF (keyset pagination pelo Id + filtro por Status casando com o covering index),
        // aqui em SQL puro para comparar o overhead de tracking/materialização do EF Core.
        var sql = @"
            SELECT TOP (@PageSize)
                p.Id,
                p.ClienteId,
                p.Status,
                p.ValorTotal,
                p.DataPedido,
                (SELECT COUNT(*) FROM ItensPedido i WHERE i.PedidoId = p.Id) AS QuantidadeItens
            FROM Pedidos p
            WHERE (@Status IS NULL OR p.Status = @Status)
              AND (@LastId IS NULL OR p.Id > @LastId)
            ORDER BY p.Id;";

        await using var connection = new SqlConnection(connectionString);

        var pedidos = await connection.QueryAsync<PedidoFastModel>(sql, new
        {
            PageSize = pageSize,
            Status = (int?)status,
            LastId = lastId
        });

        return pedidos.AsList();
    }
}
