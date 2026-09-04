using Domain.AggregatesModel.PedidoAggregate;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class PedidoRepository : BaseRepository<Pedido>, IPedidoRepository
{
    public PedidoRepository(AppDbContext context) : base(context)
    {
    }
}
