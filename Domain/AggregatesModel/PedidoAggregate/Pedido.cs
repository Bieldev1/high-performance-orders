using Domain.SeedWork;

namespace Domain.AggregatesModel.PedidoAggregate;

public class Pedido : Entity, IAggregateRoot
{
    private readonly List<ItemPedido> itens = new();

    private Pedido() { }

    public long Id { get; private set; }
    public long ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime DataPedido { get; private set; }

    public IReadOnlyCollection<ItemPedido> Itens => itens.AsReadOnly();

    public Pedido(long clienteId)
    {
        ClienteId = clienteId;
        Status = StatusPedido.Pendente;
        DataPedido = DateTime.UtcNow;
    }

    public void AdicionarItem(ItemPedido item)
    {
        itens.Add(item);
        ValorTotal += item.Quantidade * item.PrecoUnitario;
    }
}
