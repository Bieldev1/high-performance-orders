using Domain.AggregatesModel.PedidoAggregate;
using Xunit;

namespace Tests;

/// <summary>
/// Testes unitários do agregado Pedido — sem banco, sem HTTP, só a regra de domínio.
/// Contraste com PedidosControllerTests, que são de integração (sobem a API e batem no SQL Server).
/// </summary>
public class PedidoTests
{
    [Fact]
    public void NovoPedido_ComecaPendente_ComValorTotalZero_ESemItens()
    {
        var pedido = new Pedido(clienteId: 42);

        Assert.Equal(StatusPedido.Pendente, pedido.Status);
        Assert.Equal(0m, pedido.ValorTotal);
        Assert.Empty(pedido.Itens);
        Assert.Equal(42, pedido.ClienteId);
    }

    [Fact]
    public void AdicionarItem_SomaQuantidadeVezesPrecoNoValorTotal()
    {
        var pedido = new Pedido(clienteId: 1);

        pedido.AdicionarItem(new ItemPedido("Teclado", quantidade: 2, precoUnitario: 150.00m));

        Assert.Equal(300.00m, pedido.ValorTotal);
        Assert.Single(pedido.Itens);
    }

    [Fact]
    public void AdicionarItem_VariasVezes_AcumulaOValorTotal()
    {
        var pedido = new Pedido(clienteId: 1);

        pedido.AdicionarItem(new ItemPedido("Teclado", 2, 150.00m));   // 300.00
        pedido.AdicionarItem(new ItemPedido("Mouse", 1, 89.90m));      //  89.90
        pedido.AdicionarItem(new ItemPedido("Monitor", 3, 1200.00m));  // 3600.00

        Assert.Equal(3989.90m, pedido.ValorTotal);
        Assert.Equal(3, pedido.Itens.Count);
    }

    [Fact]
    public void Itens_EhSomenteLeitura_NaoDaParaMutarPorFora()
    {
        var pedido = new Pedido(clienteId: 1);

        Assert.IsNotAssignableFrom<List<ItemPedido>>(pedido.Itens);
    }
}
