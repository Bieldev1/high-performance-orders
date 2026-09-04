using Domain.AggregatesModel.PedidoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfigurations.PedidoAggregate;

internal class PedidoMap : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.Id).HasColumnName("Id").UseIdentityColumn();
        builder.Property(it => it.ClienteId).HasColumnName("ClienteId").IsRequired();
        builder.Property(it => it.Status).HasColumnName("Status").HasConversion<int>().IsRequired();
        builder.Property(it => it.ValorTotal).HasColumnName("ValorTotal").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(it => it.DataPedido).HasColumnName("DataPedido").IsRequired();

        builder.HasMany(it => it.Itens)
            .WithOne()
            .HasForeignKey(it => it.PedidoId);

        builder.Metadata.FindNavigation(nameof(Pedido.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Índices de performance NÃO são declarados aqui de propósito: são criados via SQL puro
        // em database/scripts/01-create-tables.sql, depois de analisar o execution plan das queries
        // reais — é assim que tuning de índice funciona na prática, não via Fluent API antecipada.
    }
}
