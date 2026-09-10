using Domain.SeedWork;

namespace Domain.AggregatesModel.PedidoAggregate;

public class ItemPedido : Entity
{
    // Construtor sem parâmetros usado só pelo EF Core ao materializar a entidade do banco.
    private ItemPedido() { }

    public long Id { get; private set; }
    public long PedidoId { get; private set; }
    public string NomeProduto { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    public ItemPedido(string nomeProduto, int quantidade, decimal precoUnitario)
    {
        NomeProduto = nomeProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }
}
