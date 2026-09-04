using Domain.SeedWork;

namespace Domain.AggregatesModel.ClienteAggregate;

public class Cliente : Entity, IAggregateRoot
{
    private Cliente() { }

    public long Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public Cliente(string nome, string email)
    {
        Nome = nome;
        Email = email;
        DataCadastro = DateTime.UtcNow;
    }
}
