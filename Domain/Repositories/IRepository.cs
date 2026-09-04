using Domain.SeedWork;

namespace Domain.Repositories;

public interface IRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot
{
    Task<TAggregateRoot> GetByIdAsync(object id);

    void Add(TAggregateRoot entity);

    void Update(TAggregateRoot entity);

    void Delete(TAggregateRoot entity);
}
