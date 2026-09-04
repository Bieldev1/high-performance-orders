using Domain.SeedWork;

namespace Domain.AggregatesModel.PedidoAggregate;

public class ItemPedido : Entity
{
    private ItemPedido() { }

    public long Id { get; private set; }
    public long PedidoId { get; private set; }
    public string NomeProduto { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    public ItemPedido(string nomeProduto, int quantidade, decimal precoUnitario)
    {
        NomeProduto = nomeProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }
}
