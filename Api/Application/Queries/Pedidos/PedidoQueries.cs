using Api.Application.Models.Pedidos;
using Domain.AggregatesModel.PedidoAggregate;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.Pedidos;

public class PedidoQueries : IPedidoQueries
{
    private readonly AppDbContext context;

    public PedidoQueries(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<List<PedidoSlowModel>> GetSlowAsync(int page, int pageSize)
    {
        // Problema 1: OFFSET pagination — custo cresce conforme a página aumenta, pois o SQL Server
        // precisa varrer e descartar todas as linhas anteriores ao offset.
        var pedidos = await context.Pedidos
            .OrderBy(p => p.DataPedido)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(); // sem AsNoTracking: EF passa a rastrear todas as entidades desnecessariamente

        var resultado = new List<PedidoSlowModel>();

        foreach (var pedido in pedidos)
        {
            // Problema 2 (N+1): uma query de Cliente por Pedido, em vez de trazer tudo em uma única consulta.
            var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Id == pedido.ClienteId);

            // Problema 3 (N+1): uma query de Itens por Pedido, equivalente a um Include carregado tarde demais.
            var itens = await context.ItensPedido
                .Where(i => i.PedidoId == pedido.Id)
                .ToListAsync();

            resultado.Add(new PedidoSlowModel
            {
                Id = pedido.Id,
                ClienteNome = cliente?.Nome,
                Status = pedido.Status,
                ValorTotal = pedido.ValorTotal,
                DataPedido = pedido.DataPedido,
                Itens = itens.Select(i => new ItemPedidoModel(i.NomeProduto, i.Quantidade, i.PrecoUnitario)).ToList()
            });
        }

        return resultado;
    }

    public async Task<List<PedidoFastModel>> GetFastAsync(long? lastId, int pageSize, StatusPedido? status)
    {
        var query = context.Pedidos.AsNoTracking().AsQueryable();

        // Filtro por Status antes de qualquer outra coisa: casa com a coluna líder do covering index
        // (IX_Pedidos_Covering / IX_Pedidos_Status_ValorTotal_DataPedido), permitindo Index Seek.
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        // Keyset pagination (cursor pelo Id): evita OFFSET, custo constante independente da página.
        if (lastId.HasValue)
            query = query.Where(p => p.Id > lastId.Value);

        return await query
            .OrderBy(p => p.Id)
            .Take(pageSize)
            .Select(p => new PedidoFastModel
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                Status = p.Status,
                ValorTotal = p.ValorTotal,
                DataPedido = p.DataPedido,
                // Count via IX_ItensPedido_PedidoId: index-only, sem tocar a tabela base.
                QuantidadeItens = context.ItensPedido.Count(i => i.PedidoId == p.Id)
            })
            .ToListAsync();
    }

    public async Task<BenchmarkResultModel> BenchmarkAsync(int pageSize)
    {
        var resultado = new BenchmarkResultModel();

        resultado.SlowQuery = await SqlStatisticsCapture.RunAsync(context, async () =>
        {
            context.ChangeTracker.Clear();
            await GetSlowAsync(page: 1, pageSize);
        });

        resultado.OptimizedQuery = await SqlStatisticsCapture.RunAsync(context, async () =>
        {
            context.ChangeTracker.Clear();
            await GetFastAsync(lastId: null, pageSize, status: null);
        });

        return resultado;
    }
}
