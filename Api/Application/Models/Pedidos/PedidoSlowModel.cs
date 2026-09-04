using Domain.AggregatesModel.PedidoAggregate;

namespace Api.Application.Models.Pedidos;

public class PedidoSlowModel
{
    public long Id { get; set; }
    public string? ClienteNome { get; set; }
    public StatusPedido Status { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataPedido { get; set; }
    public List<ItemPedidoModel> Itens { get; set; } = new();
}
