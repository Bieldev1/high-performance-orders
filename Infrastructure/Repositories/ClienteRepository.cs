using Domain.AggregatesModel.ClienteAggregate;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context)
    {
    }
}
