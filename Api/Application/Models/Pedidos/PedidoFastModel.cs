using Domain.AggregatesModel.PedidoAggregate;

namespace Api.Application.Models.Pedidos;

public class PedidoFastModel
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public StatusPedido Status { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataPedido { get; set; }
    public int QuantidadeItens { get; set; }
}
