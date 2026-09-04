using Domain.Repositories;
using Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public abstract class BaseRepository<TAggregateRoot> : IRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    protected readonly AppDbContext context;
    private readonly DbSet<TAggregateRoot> entities;

    protected BaseRepository(AppDbContext context)
    {
        this.context = context;
        entities = context.Set<TAggregateRoot>();
    }

    public virtual async Task<TAggregateRoot> GetByIdAsync(object id)
        => await entities.FindAsync(id);

    public virtual void Add(TAggregateRoot entity) => entities.Add(entity);

    public virtual void Update(TAggregateRoot entity) => entities.Update(entity);

    public virtual void Delete(TAggregateRoot entity) => entities.Remove(entity);
}
