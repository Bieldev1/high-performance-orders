using Domain.SeedWork;

namespace Domain.AggregatesModel.ClienteAggregate;

public class Cliente : Entity, IAggregateRoot
{
    // Construtor sem parâmetros usado só pelo EF Core ao materializar a entidade do banco;
    // os null! dizem ao compilador que o EF preenche essas propriedades via reflection.
    private Cliente() { }

    public long Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime DataCadastro { get; private set; }

    public Cliente(string nome, string email)
    {
        Nome = nome;
        Email = email;
        DataCadastro = DateTime.UtcNow;
    }
}
